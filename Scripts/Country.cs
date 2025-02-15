using Godot;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;


public partial class Country {
        public int Id;
        public string Name;
        public string Tag;
        public Color Color;
        public Dictionary<int, int> Relations = new(); //<CountryID:Relation Type> 0 -> Neutral, 1 -> Ally, 2 -> Enemy
        public ConcurrentDictionary<string, long> Production = new();
        public ConcurrentDictionary<string, long> BuildingProduction = new();
        public ConcurrentDictionary<string, long> Resources = new();
        public int _armyAttack = 1;
        public int _armyDefense = 1;
        public int _armyInitiative = 1;
        public int AttackUpgrades = 0;
        public int DefenseUpgrades = 0;
        public int InitiativeUpgrades = 0;

        public int ArmyAttack {
            get {
                return _armyAttack + (AttackUpgrades * Map.Instance.ArmyAttackScale);
            }
            set {
                _armyAttack = value;
            }
        }
        public int ArmyDefense {
            get {
                return _armyDefense + (DefenseUpgrades * Map.Instance.ArmyDefenseScale);
            }
            set {
                _armyDefense = value;
            }
        }
        public int ArmyInitiative {
            get {
                return _armyInitiative + (InitiativeUpgrades * Map.Instance.ArmyInitiativeScale);
            }
            set {
                _armyInitiative = value;
            }
        }
        
        public Country(int id, string name, string tag, Color color) {
            Id = id;
            Name = name;
            Tag = tag;
            Color = color;
            foreach(string resource in GameManager.Instance.Resources) {
                Resources[resource] = 0;
                Production[resource] = 0;
                BuildingProduction[resource] = 0;
            }
            
        }

        public static void SetRelation(Country country1, Country country2, int relation) {
            country1.Relations[country2.Id] = relation;
            country2.Relations[country1.Id] = relation;
        }

        public void Update() {
            // Collect Edge Pixels

            ConcurrentBag<Pixel> edgePixels = new();
            Parallel.For(0, Map.Instance.MapSize.X, x => {
                for(int y = 0; y < Map.Instance.MapSize.Y; y++) {
                    if(Map.Instance.Pixels[x, y].Owner != Id) {
                        continue;
                    }
                    if(x > 0 && Map.Instance.Pixels[x - 1, y].Owner != Id && edgePixels.Contains(Map.Instance.Pixels[x - 1, y]) == false) {
                        edgePixels.Add(Map.Instance.Pixels[x - 1, y]);
                    }
                    if(x < Map.Instance.MapSize.X - 1 && Map.Instance.Pixels[x + 1, y].Owner != Id && edgePixels.Contains(Map.Instance.Pixels[x + 1, y]) == false) {
                        edgePixels.Add(Map.Instance.Pixels[x + 1, y]);
                    }
                    if(y > 0 && Map.Instance.Pixels[x, y - 1].Owner != Id && edgePixels.Contains(Map.Instance.Pixels[x, y - 1]) == false) {
                        edgePixels.Add(Map.Instance.Pixels[x, y - 1]);
                    }
                    if(y < Map.Instance.MapSize.Y - 1 && Map.Instance.Pixels[x, y + 1].Owner != Id && edgePixels.Contains(Map.Instance.Pixels[x, y + 1]) == false) {
                        edgePixels.Add(Map.Instance.Pixels[x, y + 1]);
                    }
                }
            });

            // Combat
            int[] enemies = Relations.Where(relation => relation.Value == 2).Select(relation => relation.Key).ToArray();
            int enemyEdgeProvinces = 0;
            foreach(int enemy in enemies) {
                List<Pixel> enemyPixels = edgePixels.Where(pixel => pixel.Owner == enemy && Map.Instance.GetNeighboursIncludingDiagonal(pixel).Where(n => Relations[n.Owner] == 2).Count() < 6).ToList();
                enemyEdgeProvinces += enemyPixels.Count;
                Parallel.For(0, enemyPixels.Count, i => {
                    if(Resources["Gold"] < 1 || Resources["Manpower"] < 1) {
                        return;
                    }
                    int attackerInitiative = GameManager.Instance.RNG.RandiRange(0, ArmyInitiative);
                    int defenderInitiative = GameManager.Instance.RNG.RandiRange(0, Map.Instance.Countries[enemy].ArmyInitiative);
                    Production["Gold"] -= attackerInitiative * Map.Instance.NaturalResourceScale / 1000;
                    if(attackerInitiative < defenderInitiative) {
                        return;
                    }
                    if(Map.Instance.Countries[enemy].Resources["Manpower"] < 1) {
                        enemyPixels[i].Owner = Id;
                        enemyPixels[i].QueueToRefresh();
                        return;
                    }
                    int attackerAttack = GameManager.Instance.RNG.RandiRange(0, ArmyAttack);
                    int defenderDefense = GameManager.Instance.RNG.RandiRange(0, Map.Instance.Countries[enemy].ArmyDefense);
                    Production["Manpower"] -= defenderDefense * Map.Instance.NaturalResourceScale / 1000;
                    Map.Instance.Countries[enemy].Production["Manpower"] -= attackerAttack * Map.Instance.NaturalResourceScale / 1000;
                    if(attackerAttack > defenderDefense) {
                        enemyPixels[i].Owner = Id;
                        enemyPixels[i].QueueToRefresh();
                    }
                });
            }

            // Colonization
            List<Pixel> pixelsToColonize = edgePixels.Where(pixel => GameManager.Instance.RNG.RandiRange(0, 100) > 50 && pixel.Owner == 0 && pixel.PixelType != "Wasteland").ToList();
            Parallel.For(0, pixelsToColonize.Count, i => {
                pixelsToColonize[i].Owner = Id;
                pixelsToColonize[i].QueueToRefresh();
            });

            // Resource Production
            foreach(string resource in Production.Keys) {
                long resourceProduction = Production[resource] / Map.Instance.NaturalResourceScale;
                Resources[resource] += resourceProduction;
                if(resourceProduction != 0) {
                    Production[resource] = 0;
                }
                if(Resources[resource] < 0) {
                    Resources[resource] = 0;
                }
            }
            foreach(string resource in BuildingProduction.Keys) {
                long resourceProduction = BuildingProduction[resource] / Map.Instance.BuildingResourceScale;
                Resources[resource] += resourceProduction;
                if(resourceProduction != 0) {
                    BuildingProduction[resource] = 0;
                }
                if(Resources[resource] < 0) {
                    Resources[resource] = 0;
                }
            }
        }

        public void MonthlyUpdate() {
            if(Resources["Gold"] < 1 || Resources["Manpower"] < 1) {
                foreach(int target in Relations.Keys) {
                    if(Relations[target] == 2 && Map.Instance.Countries[target].Resources["Gold"] < 1 || Map.Instance.Countries[target].Resources["Manpower"] < 1) {
                        SetRelation(this, Map.Instance.Countries[target], 0);
                    }
                }
            }
            if(GameManager.Instance.RNG.RandiRange(0, 1000) < 10) {
                // Collect Edge Pixels
                ConcurrentBag<Pixel> edgePixels = new();
                Parallel.For(0, Map.Instance.MapSize.X, x => {
                    for(int y = 0; y < Map.Instance.MapSize.Y; y++) {
                        if(Map.Instance.Pixels[x, y].Owner != Id) {
                            continue;
                        }
                        if(x > 0 && Map.Instance.Pixels[x - 1, y].Owner != Id && edgePixels.Contains(Map.Instance.Pixels[x - 1, y]) == false) {
                            edgePixels.Add(Map.Instance.Pixels[x - 1, y]);
                        }
                        if(x < Map.Instance.MapSize.X - 1 && Map.Instance.Pixels[x + 1, y].Owner != Id && edgePixels.Contains(Map.Instance.Pixels[x + 1, y]) == false) {
                            edgePixels.Add(Map.Instance.Pixels[x + 1, y]);
                        }
                        if(y > 0 && Map.Instance.Pixels[x, y - 1].Owner != Id && edgePixels.Contains(Map.Instance.Pixels[x, y - 1]) == false) {
                            edgePixels.Add(Map.Instance.Pixels[x, y - 1]);
                        }
                        if(y < Map.Instance.MapSize.Y - 1 && Map.Instance.Pixels[x, y + 1].Owner != Id && edgePixels.Contains(Map.Instance.Pixels[x, y + 1]) == false) {
                            edgePixels.Add(Map.Instance.Pixels[x, y + 1]);
                        }
                    }
                });

                int[] possibleTargets = edgePixels.Where(pixel => pixel.Owner != Id).Select(pixel => pixel.Owner).Distinct().Where(owner => Relations[owner] == 0 && owner > 0).ToArray();
                if(possibleTargets.Length == 0) {
                    return;
                }
                int target = possibleTargets[GameManager.Instance.RNG.RandiRange(0, possibleTargets.Length - 1)];
                SetRelation(this, Map.Instance.Countries[target], 2);
            }
            else {
                
                if(GameManager.Instance.RNG.RandiRange(0, 1000) < 500) {
                    string possibleBuilding = GameManager.Instance.BuildingData.Keys.ToArray()[GameManager.Instance.RNG.RandiRange(0, GameManager.Instance.BuildingData.Keys.Count() - 1)];
                    bool isAtWar = Relations.Values.Where(val => val == 2).Any();
                    bool canBuild = true;

                    foreach(string resource in ((Godot.Collections.Dictionary<string, long>)GameManager.Instance.BuildingData[possibleBuilding]["Cost"]).Keys) {
                        if(Resources[resource] < ((Godot.Collections.Dictionary<string, long>)GameManager.Instance.BuildingData[possibleBuilding]["Cost"])[resource] || (isAtWar && Resources[resource] < ((Godot.Collections.Dictionary<string, long>)GameManager.Instance.BuildingData[possibleBuilding]["Cost"])[resource] * 2)) {
                            canBuild = false;
                            break;
                        }
                    }

                    if(canBuild) {
                        List<Pixel> buildablePixels = new();
                        for(int x = 0; x < Map.Instance.MapSize.X; x++) {
                            for(int y = 0; y < Map.Instance.MapSize.Y; y++) {
                                if(Map.Instance.Pixels[x, y].Building == null && Map.Instance.Pixels[x, y].Owner == Id && Map.Instance.Pixels[x, y].PixelType == "Plains") {
                                    buildablePixels.Add(Map.Instance.Pixels[x, y]);
                                }
                            }
                        }
                        int pixel = GameManager.Instance.RNG.RandiRange(0, buildablePixels.Count - 1);
                        buildablePixels[pixel].Building = new Building(possibleBuilding, buildablePixels[pixel].Position);
                        buildablePixels[pixel].QueueToRefresh();
                        foreach(string resource in ((Godot.Collections.Dictionary<string, long>)GameManager.Instance.BuildingData[possibleBuilding]["Cost"]).Keys) {
                            Resources[resource] -= ((Godot.Collections.Dictionary<string, long>)GameManager.Instance.BuildingData[possibleBuilding]["Cost"])[resource];
                        }
                    }
                }
                else {
                    if(Resources["Gold"] < Map.Instance.UpgradeCost) {
                        return;
                    }
                    Resources["Gold"] -= Map.Instance.UpgradeCost;
                    int randomNumber = GameManager.Instance.RNG.RandiRange(0, 1000);
                    if(randomNumber < 333) {
                        AttackUpgrades++;
                        return;
                    }
                    if(randomNumber < 667) {
                        DefenseUpgrades++;
                        return;
                    }
                    InitiativeUpgrades++;
                }
            }
        }

        public void YearlyUpdate() {
            
        }
    }