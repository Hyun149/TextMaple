using System.Numerics;
using System.Reflection.Emit;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Xml.Linq;

namespace TextMapleStory
{
    // 게임의 데이터를 저장하는 클래스
    class SaveData
    {
        private static SaveData? _instance;
        public Character? Player { get; set; }
        public List<Item>? Inventory { get; set; }
        public List<bool> ShopPurchases { get; set; } = new List<bool>(); // 기본적으로 빈 리스트로 초기화
        public bool HasClassChanged { get; set; } // 전직 여부 저장



        // 생성자 (기본값으로 리스트를 초기화)
        [JsonConstructor]
        private SaveData(Character player, List<Item> inventory, List<bool> shopPurchases, bool hasClassChanged)
        {
            Player = player;
            Inventory = inventory;
            ShopPurchases = shopPurchases;
            HasClassChanged = hasClassChanged;
        }

        public static SaveData Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SaveData(new Character(), new List<Item>(), new List<bool>(), false); // 기본값으로 초기화
                }
                return _instance;
            }
        }
    }

    // 캐릭터의 능력치를 정의하는 enum
    enum StatType
    {
        전투력,
        방어력,
        체력
    }

    // 장비 종류를 정의하는 enum
    enum EquipmentType
    {
        모자,
        무기,
        보조무기,
        방어구,
        장갑,
        신발,
        망토
    }

    // 게임 내 아이템을 정의하는 클래스
    class Item
    {
        [JsonPropertyName("Name")]
        public string RawName { get; set; }  // 저장용 이름

        [JsonIgnore]
        public string Name => Star > 0 ? $"{RawName} ({Star}성)" : RawName;  // 표시용 이름

        public string Description { get; set; }
        public int Power { get; set; }
        public StatType Type { get; set; }
        public EquipmentType EquipmentType { get; set; }
        public bool IsEquipped { get; set; } = false;
        public int Star { get; set; } = 0;

        public int Price => Power * 1000;

        public Item(string name, StatType type, int power, string description, EquipmentType equipmentType)
        {
            RawName = name;
            Type = type;
            Power = power;
            Description = description;
            EquipmentType = equipmentType;
        }
    }

    // 플레이어 캐릭터의 상태를 정의하는 클래스
    class Character
    {
        public string Name { get; set; } = "함장";
        public string Job { get; set; } = "초보자";
        public int Level { get; set; } = 1;
        public int Attack { get; set; } = 10;
        public int Defense { get; set; } = 5;
        public int HP { get; set; } = 100;
        public int MaxHP { get; set; } = 100;  // 최대 체력 추가
        public long Meso { get; set; } = 1000000;
        public int Exp { get; set; } = 0;
        public int MaxExp { get; set; } = 100;
        public bool HasClassChanged { get; set; } = false;
        public List<Item> Inventory { get; set; } = new List<Item>();  // 인벤토리 추가

        // 경험치 추가 및 레벨업 처리
        public void AddExp(int amount)
        {
            Exp += amount;
            Console.WriteLine($"{Name}은 {amount}의 경험치를 얻었습니다!");

            // 레벨업 조건 체크
            while (Exp >= MaxExp)
            {
                Exp -= MaxExp;  // 남은 경험치
                Level++;
                MaxExp += 10;  // 각 레벨업마다 필요한 경험치를 증가시킬 수 있습니다.
                Console.WriteLine($"{Name}은 레벨 {Level}로 상승했습니다!");

                // 레벨업 시 기본 공격력과 방어력 증가
                Attack += 5;  // 기본 공격력 5 증가
                Defense += 5;  // 기본 방어력 5 증가
                MaxHP += 5;
                Console.WriteLine($"{Name}의 능력치가 5씩 증가했습니다! (전투력: {Attack}, 방어력: {Defense}, 체력: {MaxHP})");

                // 1차 전직 조건 (레벨 10 이상)
                if (Level >= 10 && HasClassChanged == false)
                {
                    Console.WriteLine($"{Name}은 1차 전직을 할 수 있습니다!");
                }
            }
        }
        // 1차 전직을 처리하는 함수
        public void ClassChange()
        {
            if (Level >= 10)
            {
                Console.WriteLine($"1차 전직을 진행합니다. {Name}의 직업이 변경됩니다.");

                // 전직 후 직업 변경 및 능력치 강화
                Job = "마법사";
                Attack += 15;  // 전직 후 능력치 증가
                Defense += 15;
                MaxHP += 15;
                Console.WriteLine($"{Name}의 직업이 {Job}로 변경되었습니다!\n전투력: {Attack}, 방어력: {Defense}, 체력: {MaxHP}"); // "스킬획득: 매직클로"

                HasClassChanged = true;  // 전직 완료 후 전직 여부를 true로 설정
            }
            else if (HasClassChanged)  // 이미 전직을 완료한 경우
            {
                Console.WriteLine($"{Name}은 이미 1차 전직을 완료했습니다.");
            }
            else
            {
                Console.WriteLine("1차 전직을 위해서는 레벨 10 이상이 되어야 합니다.");
            }
        }
    }

    // 상점 아이템을 정의하는 클래스
    class ShopItem
    {
        public Item ItemData { get; }
        public bool IsPurchased { get; set; }

        // 아이템 가격 계산
        public int Price
        {
            get { return ItemData.Power * 1000; }  // power * 1000을 가격으로 설정
        }

        // 상점 아이템 생성자
        public ShopItem(Item item)
        {
            ItemData = item;
            IsPurchased = false;
        }

        // 아이템을 구매하는 함수
        public void Purchase()
        {
            IsPurchased = true;

            SaveData.Instance.ShopPurchases.Add(true); // 아이템 구매 상태 저장
        }
    }

    // 몬스터를 정의하는 클래스
    class Monster
    {
        public string Name { get; set; }
        public int HP { get; set; } = 50;
        public int Attack { get; set; } = 10;
        public int Defense { get; set; } = 5;
        public int ExpReward { get; set; } = 25;
        public int MesoDrop { get; set; } = 5000;
        public List<Item> DroppedItems { get; set; } = new List<Item>(); // 드롭할 아이템들

        // 몬스터 생성자
        public Monster(string name, int hp, int attack, int defense, int expReward, int mesoDrop)
        {
            Name = name;
            HP = hp;
            Attack = attack;
            Defense = defense;
            ExpReward = expReward;
            MesoDrop = mesoDrop;
        }

        // 몬스터 처치 후 경험치와 메소를 주는 메서드
        public void DropRewards(Character player)
        {
            player.AddExp(ExpReward);
            player.Meso += MesoDrop;  // 드랍된 메소를 플레이어에게 추가
            Console.WriteLine($"{Name}은(는) {MesoDrop} 메소를 드랍했습니다!\n경험치: {player.Exp + ExpReward} / {player.MaxExp}");

            Random rand = new Random();
            double dropChance = rand.NextDouble();  // 0과 1 사이의 실수값 반환

            if (dropChance <= 0.1)
            {
                // 머쉬맘의 포자 드롭
                if (Name == "머쉬맘")
                {
                    var mushyMushroomSpores = new Item("머쉬맘의 포자", StatType.체력, 50, "[튜토리얼 보스]머쉬맘의 포자이다. 머리에 쓰면 든든할 것 같다.", EquipmentType.모자);
                    AddDroppedItem(mushyMushroomSpores, player);
                }
            }

            foreach (var item in DroppedItems)
            {
                Console.WriteLine($"{item.Name}을(를) 드랍했습니다!");
            }
        }

        // 아이템 드롭 설정
        public void AddDroppedItem(Item item, Character player)  // player 객체를 매개변수로 받음
        {
            DroppedItems.Add(item);
            Console.WriteLine($"{item.Name}을(를) DroppedItems에 추가했습니다!");

            if (!player.Inventory.Contains(item))  // 중복 방지
            {
                player.Inventory.Add(item);  // inventory에 아이템 추가
                Console.WriteLine($"{item.Name}이(가) 플레이어 인벤토리에 추가되었습니다.");
            }
        }

        // 몬스터가 공격하는 메서드
        public int AttackPlayer(Character player)
        {
            int damage = Math.Max(1, this.Attack - player.Defense);  // 피해가 최소 1 이상
            return damage;
        }
    }


    // 프로그램의 진입점(entry point)인 클래스
    // 이 클래스는 프로그램 실행 시 가장 먼저 실행되는 메서드를 포함하고 있습니다.
    internal class TextMapleStory
    {
        public static string GetFormattedMeso() => player.Meso.ToString("N0");
        public static string FormatPrice(long Price) => Price.ToString("N0");

        static Character player = new Character();
        static List<Item> inventory = new List<Item>();
        static List<ShopItem> shopItems = new List<ShopItem>();
        static Timer? autoSaveTimer;

        // 프로그램 시작점 함수
        static void Main()
        {
            // AutoSave 호출: 프로그램 시작 시 10분마다 자동 저장 시작
            AutoSave();

            try
            {
                // 저장된 게임 불러오기
                LoadGame();

                // 초기 인벤토리 설정
                if (player.Inventory == null || player.Inventory.Count == 0)
                {
                    player.Inventory = new List<Item>
            {
                new Item("허름한 셔츠", StatType.방어력, 3, " ", EquipmentType.방어구),
                new Item("허름한 모자", StatType.체력, 3, " ", EquipmentType.모자),
                new Item("나무 스태프", StatType.전투력, 3, " ", EquipmentType.무기)
            };
                }

                // 게임 흐름을 보여주는 화면
                ShowMainMenu();

            }
            catch (Exception ex)
            {
                Console.WriteLine($"게임 데이터를 불러오는 중 오류 발생: {ex.Message}");
                Console.WriteLine("기본 설정으로 게임을 시작합니다.");
            }
            finally
            {
                // 항상 실행됨 (정상 종료든 예외든)
                SaveGame();
                Console.WriteLine("[게임이 저장되었습니다.]");

                autoSaveTimer?.Dispose(); // 자동 저장 타이머 종료
            }
        }


        // 10분마다 자동 저장하는 함수
        static void AutoSave()
        {
            if (autoSaveTimer != null) return;  // 이미 타이머가 실행 중이라면 새로 시작하지 않음

            autoSaveTimer = new Timer(e =>
            {
                Console.WriteLine("[자동 저장 중...]");
                SaveGame();
            }, null, TimeSpan.Zero, TimeSpan.FromMinutes(10)); // 10분마다 자동 저장
        }

        // 게임의 첫 인트로 메시지를 출력하는 함수
        static void ShowIntro()
        {
            Console.WriteLine("=======================================================================================");
            Console.WriteLine("Text Maple Story에 오신 것을 환영합니다! ");
            Console.WriteLine("이곳에서 사냥터 또는 보스던전으로 들어가기 전 정비를 하실 수 있는 헤네시스 마을입니다.");
            Console.WriteLine("=======================================================================================");
        }

        // 메인 메뉴를 출력하는 함수
        static void ShowMainMenu()
        {
            ShowIntro();

            while (true)
            {
                // 전직을 한 경우 1차 전직 메뉴를 숨김
                if (player.HasClassChanged)
                {
                    Console.WriteLine("1. 캐릭터 정보\n2. 인벤토리\n3. 상점\n4. 헤네시스 사냥터\n5. 피로회복온천\n0. 게임종료");
                }
                else
                {
                    Console.WriteLine("1. 캐릭터 정보\n2. 인벤토리\n3. 상점\n4. 헤네시스 사냥터\n5. 피로회복온천\n6. 1차 전직\n0. 게임종료");
                }

                Console.Write("\n>> ");
                string? input = Console.ReadLine();

                switch (input)
                {
                    case "1":
                        Console.WriteLine("\n[캐릭터 정보 화면으로 이동합니다...]\n");
                        ShowStatus();
                        break;

                    case "2":
                        Console.WriteLine("\n[인벤토리 화면으로 이동합니다...]\n");
                        ShowInventory();
                        break;

                    case "3":
                        Console.WriteLine("\n[상점 화면으로 이동합니다...]\n");
                        ShowShop();
                        break;

                    case "4":
                        Console.WriteLine("\n[헤네시스 사냥터 화면으로 이동합니다...]\n");
                        ShowHenesysZones();
                        break;

                    case "5":
                        HeathRecovery();
                        break;

                    case "6":
                        // 레벨 10 이상일 때 전직 가능
                        if (player.Level >= 10)
                        {
                            Console.WriteLine("\n[1차 전직을 진행합니다...]\n");
                            player.ClassChange();  // 전직 함수 호출
                        }
                        else
                        {
                            Console.WriteLine("\n레벨 10 이상이 되어야 1차 전직이 가능합니다.\n");
                        }
                        break;

                    case "0":
                        Console.WriteLine("\n[게임을 종료합니다...]\n");
                        SaveGame(); // 게임 종료 시 자동 저장
                        Environment.Exit(0); // 프로그램 종료
                        break;

                    default:
                        Console.WriteLine("잘못된 입력입니다.\n");
                        break;
                }
            }
        }

        // 휴식하기 기능을 구현한 함수
        static void HeathRecovery()
        {

            int totalMaxHP = player.MaxHP + player.Inventory.Where(i => i.IsEquipped && i.Type == StatType.체력).Sum(i => i.Power);

            Console.WriteLine($"'500'메소를 지불하시면 파워엘릭서를 드립니다. (보유 메소: {player.Meso}");

            if (player.Meso >= 500)
            {
                player.HP = Math.Min(player.HP + (totalMaxHP - player.HP), totalMaxHP);
                player.Meso -= 500;

                Console.WriteLine($"\n[파워엘릭서의 강력함으로 체력을 전부 회복합니다...]\n\n현재 체력: {player.HP} / {totalMaxHP}\n남은 메소: {player.Meso}");
            }
            else
            {
                // 메소가 부족한 경우
                Console.WriteLine("메소가 부족하여 체력을 회복할 수 없습니다.");
            }
            ShowMainMenu();
        }

        // 캐릭터의 정보를 출력하는 함수
        static void ShowStatus()
        {
            int totalAttack = player.Attack; // 기본값으로 player의 공격력
            int totalDefense = player.Defense; // 기본값으로 player의 방어력
            int totalMaxHP = player.MaxHP;

            if (player.Inventory != null)
            {
                // inventory가 null이 아닌 경우에만 추가적인 계산
                totalAttack += player.Inventory.Where(i => i != null && i.IsEquipped && i.Type == StatType.전투력).Sum(i => i.Power);
                totalDefense += player.Inventory.Where(i => i != null && i.IsEquipped && i.Type == StatType.방어력).Sum(i => i.Power);
                totalMaxHP += player.Inventory.Where(i => i != null && i.IsEquipped && i.Type == StatType.체력).Sum(i => i.Power);
            }
            else
            {
                // inventory가 null인 경우 처리 로직
                Console.WriteLine("player.Inventory가 null입니다.");
            }

            player.HP = Math.Min(player.HP, totalMaxHP); // 체력이 totalMaxHP를 넘지 않도록 제한
            player.HP = Math.Max(player.HP, 0);

            // 캐릭터 정보 출력
            Console.WriteLine("캐릭터 정보\n");
            Console.WriteLine($"{player.Name} ( {player.Job} )");
            Console.WriteLine($"Lv. {player.Level}");
            Console.WriteLine($"전투력: {player.Attack} (+{totalAttack - player.Attack}) = {totalAttack}");
            Console.WriteLine($"방어력: {player.Defense} (+{totalDefense - player.Defense}) = {totalDefense}");
            Console.WriteLine($"체력: {player.MaxHP} (+{totalMaxHP - player.MaxHP}) = {player.HP} / {totalMaxHP}");
            Console.WriteLine($"경험치: {player.Exp} / {player.MaxExp}");
            Console.WriteLine($"보유 메소:  {GetFormattedMeso()}메소\n");

            // 메뉴 출력
            Console.WriteLine("0. 나가기");
            Console.WriteLine("\n>> ");
            string? input = Console.ReadLine();

            if (input == "0")
            {
                Console.WriteLine("\n[마을로 돌아갑니다...]\n");
                ShowMainMenu();
            }
            else
            {
                Console.WriteLine("잘못된 입력입니다.");
            }
        }

        // 인벤토리 화면을 출력하는 함수
        static void ShowInventory()
        {
            Console.WriteLine($"인벤토리\n보유 중인 아이템을 사용할 수 있습니다.\n보유 메소:  {GetFormattedMeso()}메소\n\n[인벤토리 목록]");

            if (player.Inventory.Count == 0)
            {
                Console.WriteLine("(아이템이 없습니다.)\n");
            }
            else
            {
                Console.WriteLine("인벤토리 아이템 수: " + player.Inventory.Count);
                PrintInventory();
            }

            Console.WriteLine("1. 장착 관리\n2. 강화\n0.나가기");
            Console.Write("\n>> ");
            string? input = Console.ReadLine();

            switch (input)
            {
                case "1":
                    ManageEquipment();
                    break;

                case "2":
                    ItemUpgrade(); // 아이템 강화 처리
                    break;

                case "0":
                    Console.WriteLine("\n[마을로 돌아갑니다...]\n");
                    ShowMainMenu();
                    break;

                default:
                    Console.WriteLine("잘못된 입력입니다.\n");
                    break;
            }
        }

        static void ItemUpgrade()
        {
            Console.WriteLine($"\n강화를 시작하시겠습니까?\n보유 메소: {GetFormattedMeso()} 메소");

            Console.WriteLine("\n[강화 가능한 아이템 목록]:");
            for (int i = 0; i < player.Inventory.Count; i++)
            {
                var item = player.Inventory[i];
                int itemPrice = item.Power * (1 + item.Star) * 100;
                Console.WriteLine($"{i + 1}. {item.Name} | {item.Type} | 능력치: {item.Power} | 강화 비용: {FormatPrice(itemPrice)} 메소");
            }

            Console.WriteLine("\n강화할 아이템 번호를 입력해주세요:");
            string? input = Console.ReadLine();
            if (!int.TryParse(input, out int choice) || choice < 1 || choice > player.Inventory.Count)
            {
                Console.WriteLine("잘못된 입력입니다.");
                ShowInventory();
                return;
            }

            var selectedItem = player.Inventory[choice - 1];
            int upgradePrice = selectedItem.Power * (1 + selectedItem.Star) * 100;

            if (selectedItem.Star >= 30)
            {
                Console.WriteLine("이 아이템은 이미 최대 강화 수치(30성)에 도달했습니다.");
                return;
            }
            else if (player.Meso < upgradePrice)
            {
                Console.WriteLine("메소가 부족합니다.");
                return;
            }

            Random rand = new Random();
            int successChance = 80 - (selectedItem.Star * 2);
            int failChance = 15 + (selectedItem.Star * 2);
            int downgradeChance = 5 + (selectedItem.Star);
            int statBoost = rand.Next(1, 4) * selectedItem.Star;
            int randomValue = rand.Next(1, 101);

            Console.WriteLine($"\n성공 확률: {successChance}%, 실패: {failChance}%, 하락: {downgradeChance}%\n");

            if (randomValue <= successChance)
            {
                selectedItem.Power += statBoost;
                selectedItem.Star++;
                Console.WriteLine($"{selectedItem.Name} 강화에 성공했습니다! 능력치 +{statBoost} → 현재 능력치: {selectedItem.Power}");
            }
            else if (randomValue <= successChance + failChance)
            {
                Console.WriteLine($"{selectedItem.Name} 강화에 실패했습니다.");
            }
            else
            {
                selectedItem.Power = Math.Max(1, selectedItem.Power - selectedItem.Star);
                Console.WriteLine($"{selectedItem.Name} 강화에 실패하여 능력치가 하락했습니다. 현재 능력치: {selectedItem.Power}");
            }

            player.Meso -= upgradePrice;
            Console.WriteLine($"[{FormatPrice(upgradePrice)}] 메소가 사용되었습니다.");

            ShowInventory();
        }


        // 아이템을 장착 관리하는 함수
        static void ManageEquipment()
        {
            Console.WriteLine("인벤토리 - 장착 관리\n 보유 중인 아이템을 관리할 수 있습니다.\n[아이템 목록]");

            // 장착할 수 있는 아이템만 필터링
            var equipableItems = player.Inventory.Where(i => i.EquipmentType == EquipmentType.무기 ||
                                               i.EquipmentType == EquipmentType.방어구 ||
                                               i.EquipmentType == EquipmentType.모자 ||
                                               i.EquipmentType == EquipmentType.보조무기 ||
                                               i.EquipmentType == EquipmentType.신발 ||
                                               i.EquipmentType == EquipmentType.망토 ||
                                               i.EquipmentType == EquipmentType.장갑)
                                  .ToList();

            // 아이템 목록 출력
            for (int i = 0; i < equipableItems.Count; i++)
            {
                Item item = equipableItems[i];
                string equippedMark = item.IsEquipped ? "[E]" : "";
                Console.WriteLine($"{i + 1}. {equippedMark} {item.Name} | {item.Description} | {item.EquipmentType}");
            }

            Console.WriteLine("0. 나가기\n\n>> ");
            string? input = Console.ReadLine();

            if (int.TryParse(input, out int index))
            {
                if (index == 0)
                {
                    Console.WriteLine("\n[인벤토리로 돌아갑니다...]\n");
                    ShowInventory();
                }
                else if (index >= 1 && index <= equipableItems.Count)
                {
                    Item selectedItem = equipableItems[index - 1];  // 다시 선언 없이 기존 selectedItem 사용

                    // 장착하려는 아이템이 이미 장착된 상태라면
                    if (selectedItem.IsEquipped)
                    {
                        // 장착된 아이템을 해제
                        selectedItem.IsEquipped = false;
                        var itemToUpdate = player.Inventory.First(i => i.Name == selectedItem.Name);
                        itemToUpdate.IsEquipped = false;

                        Console.WriteLine($"{selectedItem.Name}을(를) 해제했습니다.");
                        ManageEquipment(); // 장착 관리 화면을 다시 출력
                    }
                    else
                    {
                        // 동일한 아이템 타입을 가진 기존 장착 아이템 해제
                        var equippedItem = player.Inventory.FirstOrDefault(i => i.IsEquipped && i.EquipmentType == selectedItem.EquipmentType);
                        if (equippedItem != null)
                        {
                            equippedItem.IsEquipped = false;
                            Console.WriteLine($"{equippedItem.Name}을(를) 해제하고 {selectedItem.Name}을(를) 장착합니다.");
                        }

                        // 새로운 아이템을 장착
                        selectedItem.IsEquipped = true;

                        // 장착된 아이템을 업데이트
                        var itemToUpdate = player.Inventory.First(i => i.Name == selectedItem.Name);
                        itemToUpdate.IsEquipped = selectedItem.IsEquipped;

                        Console.WriteLine($"{selectedItem.Name}을(를) 장착했습니다.");
                        ManageEquipment(); // 장착 관리 화면을 다시 출력
                    }
                }
                else
                {
                    Console.WriteLine("잘못된 입력입니다.\n");
                    ManageEquipment();
                }
            }
            else
            {
                Console.WriteLine("잘못된 입력입니다.\n");
                ManageEquipment();
            }
        }


        // 인벤토리에 있는 아이템 목록을 출력하는 함수
        static void PrintInventory(bool showIndex = false)
        {
            if (player.Inventory.Count == 0)  // player.Inventory로 변경
            {
                Console.WriteLine("(아이템이 없습니다.)");
                return;
            }

            for (int i = 0; i < player.Inventory.Count; i++)  // player.Inventory로 변경
            {
                // 인덱스가 유효한지 확인
                if (i >= player.Inventory.Count)  // 인덱스 범위 체크
                {
                    Console.WriteLine("아이템 인덱스가 범위를 초과했습니다.");
                    break;
                }

                Item item = player.Inventory[i];  // player.Inventory로 변경
                string equippedMark = item.IsEquipped ? "[E]" : "";
                string prefix = showIndex ? $"{i + 1}. " : "- ";
                Console.WriteLine($"{prefix}{equippedMark}{item.Name} | " +
                                  $"{item.Type} : {item.Power} | " +
                                  $"설명: {item.Description} | " +
                                  $"장비타입: {Enum.GetName(typeof(EquipmentType), item.EquipmentType)}");
            }

            Console.WriteLine();
        }

        // 상점 화면을 출력하는 함수
        static void ShowShop()
        {
            Console.WriteLine($"상점\n필요한 아이템을 얻을 수 있는 상점입니다.\n\n\n[보유 메소]: {GetFormattedMeso()}메소\n\n\n[구매 가능한 아이템 목록]");
            for (int i = 0; i < shopItems.Count; i++)
            {
                var shopItem = shopItems[i];
                string name = shopItem.ItemData.Name;
                string type = shopItem.ItemData.Type.ToString();
                int power = shopItem.ItemData.Power;
                string desc = shopItem.ItemData.Description;
                string priceText = shopItem.IsPurchased ? "구매완료" : $"{shopItem.Price} 메소";
                string equipmentType = shopItem.ItemData.EquipmentType.ToString();

                Console.WriteLine($"- {i + 1} {name} | {type} +{power} | {desc} | {equipmentType} | {priceText}");
            }

            Console.WriteLine("\n1. 아이템 구매\n2. 아이템 판매\n0. 나가기\n\n원하시는 행동을 입력해주세요.\n>> ");
            string? input = Console.ReadLine();

            switch (input)
            {
                case "1":
                    HandlePurchase();
                    break;

                case "2":
                    HandleSale();  // 판매 기능 처리
                    break;

                case "0":
                    Console.WriteLine("\n[마을로 돌아갑니다...]\n");
                    ShowMainMenu();
                    break;

                default:
                    Console.WriteLine("잘못된 입력입니다.\n");
                    ShowShop();
                    break;
            }

            // 아이템 구매 처리 함수
            static void HandlePurchase()
            {
                Console.WriteLine("\n[구매 가능한 아이템 목록]");
                for (int i = 0; i < shopItems.Count; i++)
                {
                    var shopItem = shopItems[i];
                    string name = shopItem.ItemData.Name;
                    string type = shopItem.ItemData.Type.ToString();
                    int power = shopItem.ItemData.Power;
                    string desc = shopItem.ItemData.Description;
                    string priceText = shopItem.IsPurchased ? "구매완료" : $"{shopItem.Price} 메소";
                    string equipmentType = shopItem.ItemData.EquipmentType.ToString();
                    Console.WriteLine($"- {i + 1} {name} | {type} +{power} | {desc} | {equipmentType} | {priceText}");
                }

                Console.WriteLine("0. 나가기\n\n원하시는 아이템 번호를 입력해주세요.\n>> ");
                string? input = Console.ReadLine();

                if (int.TryParse(input, out int index))
                {
                    if (index == 0)
                    {
                        Console.WriteLine("\n[상점으로 돌아갑니다...]\n");
                        ShowShop();
                        return;
                    }

                    if (index < 1 || index > shopItems.Count)
                    {
                        Console.WriteLine("잘못된 입력입니다.\n");
                        HandlePurchase();
                        return;
                    }

                    var selectedItem = shopItems[index - 1];

                    if (selectedItem.IsPurchased)
                    {
                        Console.WriteLine("이미 구매한 아이템입니다.\n");
                    }
                    else if (player.Meso >= selectedItem.Price)
                    {
                        player.Meso -= selectedItem.Price;
                        player.Inventory.Add(selectedItem.ItemData);
                        selectedItem.Purchase();

                        Console.WriteLine($"{selectedItem.ItemData.Name}을(를) 구매했습니다!\n");
                    }
                    else
                    {
                        Console.WriteLine("메소가 부족합니다.\n");
                    }
                }
                else
                {
                    Console.WriteLine("잘못된 입력입니다.\n");
                }

                // 구매 후 다시 목록 출력
                HandlePurchase();
            }
        }

        // 아이템 판매 처리 함수
        static void HandleSale()
        {
            Console.WriteLine("\n[판매 가능한 아이템 목록]");

            // 판매할 수 있는 아이템만 필터링 (장착되지 않은 아이템들)
            var sellableItems = player.Inventory.Where(i => !i.IsEquipped).ToList();  // player.Inventory 사용

            // 판매할 아이템이 없을 경우
            if (sellableItems.Count == 0)
            {
                Console.WriteLine("판매할 아이템이 없습니다.");
                ShowShop();
                return;
            }

            // 판매할 아이템 목록 출력
            for (int i = 0; i < sellableItems.Count; i++)
            {
                var item = sellableItems[i];
                // 판매가격 계산: Power * 500
                int salePrice = item.Power * 500;  // 가격 계산 수정
                Console.WriteLine($"{i + 1}. {item.Name} | {item.Type} +{item.Power} | 판매가격: {salePrice} 메소 | {item.Description}");
            }

            Console.WriteLine("0. 나가기\n\n판매할 아이템 번호를 입력해주세요.\n>> ");
            string? input = Console.ReadLine();

            // 아이템 번호를 선택한 경우
            if (int.TryParse(input, out int index))
            {
                if (index == 0)
                {
                    ShowShop();  // 상점으로 돌아가기
                }
                else if (index >= 1 && index <= sellableItems.Count)
                {
                    var selectedItem = sellableItems[index - 1];

                    // 아이템 판매 가격 계산: Power * 500
                    int salePrice = selectedItem.Power * 500;

                    // 메소 추가
                    player.Meso += salePrice;

                    // 아이템을 인벤토리에서 제거
                    player.Inventory.Remove(selectedItem);  // player.Inventory에서 제거

                    Console.WriteLine($"{selectedItem.Name}을(를) 판매하여 {salePrice} 메소를 얻었습니다.\n");
                    ShowShop();
                }
                else
                {
                    Console.WriteLine("잘못된 입력입니다.\n");
                    HandleSale();  // 잘못된 입력 처리
                }
            }
            else
            {
                Console.WriteLine("잘못된 입력입니다.\n");
                HandleSale();  // 잘못된 입력 처리
            }
        }

        // 게임 데이터를 저장하는 함수
        static void SaveGame()
        {
            SaveData data = SaveData.Instance;  // Singleton 인스턴스를 사용합니다.

            data.Player = player;
            data.Inventory = player.Inventory;
            data.ShopPurchases = shopItems.Select(item => item.IsPurchased).ToList();
            data.HasClassChanged = player.HasClassChanged;  // 전직 여부 저장

            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });

            string filePath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())?.FullName ?? Directory.GetCurrentDirectory(), "savegame.json");
            File.WriteAllText(filePath, json);

            Console.WriteLine($"게임 데이터가 저장되었습니다: {filePath}");
        }

        // 게임 데이터를 로드하는 함수
        static void LoadGame()
        {
            string filePath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())?.FullName ?? Directory.GetCurrentDirectory(), "savegame.json");

            if (!File.Exists(filePath))
                return;

            string json = File.ReadAllText(filePath);
            SaveData? data = JsonSerializer.Deserialize<SaveData>(json);

            if (data != null)
            {
                // 기본 캐릭터 정보
                player = data.Player ?? new Character();
                player.Inventory = data.Inventory ?? new List<Item>();
                player.Attack = data.Player?.Attack ?? 10;
                player.Defense = data.Player?.Defense ?? 5;
                player.Exp = data.Player?.Exp ?? 0;
                player.Level = data.Player?.Level ?? 1;
                player.HasClassChanged = data.HasClassChanged;
                player.Job = player.HasClassChanged ? "마법사" : "초보자";

                // 상점 아이템 구성 (※ Main에서는 하지 않음)
                shopItems = new List<ShopItem>
                {
            new ShopItem(new Item("머쉬맘의 포자", StatType.체력, 50, "[튜토리얼 보스]머쉬맘의 포자이다. 머리에 쓰면 든든할 것 같다.", EquipmentType.모자)),
            new ShopItem(new Item("화이트 도로스 로브", StatType.방어력, 29, "흰 천으로 만들어진 로브입니다.", EquipmentType.방어구)),
            new ShopItem(new Item("고목나무 스태프", StatType.전투력, 35, "고목나무로 만들어진 스태프입니다.", EquipmentType.무기))
                };

                // 구매 여부 반영
                for (int i = 0; i < data.ShopPurchases.Count && i < shopItems.Count; i++)
                {
                    shopItems[i].IsPurchased = data.ShopPurchases[i];
                }
            }
        }

        // 전투 기능 처리 함수
        static void Battle(Monster monster)
        {
            while (player.HP > 0 && monster.HP > 0)
            {
                Console.WriteLine($"{monster.Name}의 체력: {monster.HP}\n{player.Name}의 체력: {player.HP}\n1. 공격\n2. 방어\n0. 도망");
                string? input = Console.ReadLine();

                if (input == "1")  // 공격
                {
                    int totalAttack = player.Attack + player.Inventory.Where(i => i.IsEquipped && i.Type == StatType.전투력).Sum(i => i.Power);
                    int totalDefense = player.Defense + player.Inventory.Where(i => i.IsEquipped && i.Type == StatType.방어력).Sum(i => i.Power);
                    int totalMaxHP = player.MaxHP + player.Inventory.Where(i => i.IsEquipped && i.Type == StatType.체력).Sum(i => i.Power);

                    int damage = totalAttack - monster.Defense;
                    damage = damage > 0 ? damage : 1;  // 피해가 최소 1이 되도록
                    monster.HP -= damage;
                    Console.WriteLine($"{player.Name}이 {monster.Name}에게 {damage}의 피해를 입혔습니다!");

                    if (monster.HP <= 0)
                    {
                        Console.WriteLine($"{monster.Name}을 처치했습니다!");
                        monster.DropRewards(player);
                        Console.WriteLine("\n[전투를 계속합니다...]\n");
                        ShowHenesysZones();
                        break;
                    }

                    damage = monster.AttackPlayer(player);
                    player.HP -= damage;
                    Console.WriteLine($"{monster.Name}이 {player.Name}에게 {damage}의 피해를 입혔습니다!");

                    if (player.HP <= 0)
                    {
                        player.HP = (int)(totalMaxHP * 0.5);
                        Console.WriteLine($"{player.Name}은 쓰러졌습니다. 마을로 이송됩니다.\n{player.Name}의 체력이 {player.HP}로 회복되었습니다.");
                        ShowMainMenu();
                        break;
                    }
                }
                else if (input == "0")  // 도망
                {
                    Console.WriteLine("[전투에서 도망쳤습니다.]\n\n[마을로 돌아갑니다...]\n");
                    ShowMainMenu();
                    break;
                }
            }
        }

        // 사냥터 화면을 출력하는 함수
        static void ShowHenesysZones()
        {
            Console.WriteLine("=============================================");
            Console.WriteLine("튜토리얼 사냥터를 선택해주세요:");
            Console.WriteLine("1. 포자언덕");
            Console.WriteLine("2. 콧노래 오솔길");
            Console.ForegroundColor = ConsoleColor.Magenta; // 보라색으로 설정
            Console.WriteLine("[튜토리얼 보스]"); // 보라색 텍스트
            Console.ResetColor(); // 색상 리셋
            Console.WriteLine("3. 머쉬맘의 오솔길");
            Console.WriteLine("0. 마을로 돌아가기");
            Console.WriteLine("=============================================");
            Console.Write("\n>> ");

            string? input = Console.ReadLine();

            switch (input)
            {
                case "1":
                    EnterHenesysZone("포자언덕", "스포아", 50, 10, 5, 25, 500);  // 포자언덕
                    break;
                case "2":
                    EnterHenesysZone("콧노래 오솔길", "초록버섯", 100, 20, 10, 50, 1000);  // 콧노래 오솔길
                    break;
                case "3":
                    Console.ForegroundColor = ConsoleColor.Magenta; // 보라색으로 설정
                    Console.WriteLine("[튜토리얼 보스]"); // 보라색 텍스트
                    Console.ResetColor(); // 색상 리셋
                    EnterHenesysZone("머쉬맘의 오솔길", "머쉬맘", 1000, 600, 100, 500, 10000);  // 머쉬맘의 오솔길
                    break;
                case "0":
                    ShowMainMenu();
                    break;
                default:
                    Console.WriteLine("잘못된 입력입니다.");
                    ShowHenesysZones();
                    break;
            }
        }

        // 사냥터에 입장하여 몬스터와 전투를 시작하는 함수
        static void EnterHenesysZone(string zoneName, string monsterName, int hp, int attack, int defense, int expReward, int mesoDrop)
        {
            Console.WriteLine($"{zoneName}에 도달했습니다!");
            Console.WriteLine($"'{monsterName}'이 나타났습니다!");
            Monster monster = new Monster(monsterName, hp, attack, defense, expReward, mesoDrop);

            Battle(monster);  // 전투 시작
        }
    }
}