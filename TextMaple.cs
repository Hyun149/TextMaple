using System.Numerics;
using System.Reflection.Emit;
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
        public string Name { get; set; }
        public string Description { get; set; }
        public int Power { get; set; }
        public StatType Type { get; set; } // enum 타입으로 변경
        public EquipmentType EquipmentType { get; set; } // 새로운 장비 타입
        public bool IsEquipped { get; set; } = false;
        public int Price
        {
            get { return Power * 1000; } // 전투력 * 1000을 가격으로 설정
        }

        // 아이템 생성자
        public Item(string name, StatType type, int power, string description, EquipmentType equipmentType)
        {
            Name = name;
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
        public long Meso { get; set; } = 10000;
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
                Console.WriteLine($"{Name}의 직업이 {Job}로 변경되었습니다!");
                Console.WriteLine($"전투력: {Attack}, 방어력: {Defense}, 체력: {MaxHP})"); // "스킬획득: 매직클로"

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

            foreach (var item in DroppedItems)
            {
                player.Inventory.Add(item);
                Console.WriteLine($"{item.Name}을(를) 드랍했습니다!");
            }
        }

        // 아이템 드롭 설정
        public void AddDroppedItem(Item item)
        {
            DroppedItems.Add(item);
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

                // 상점아이템 세팅 (불러온 후에 설정)
                shopItems.Add(new ShopItem(new Item("머쉬맘의 포자", StatType.체력, 50, "[튜토리얼 보스]머쉬맘의 포자이다. 왠지 머리에 쓰면 든든할 것 같다.", EquipmentType.모자)));
                shopItems.Add(new ShopItem(new Item("화이트 도로스 로브", StatType.방어력, 29, "흰 천으로 만들어진 로브입니다.", EquipmentType.방어구)));
                shopItems.Add(new ShopItem(new Item("고목나무 스태프", StatType.전투력, 35, "고목나무로 만들어진 스태프입니다.", EquipmentType.무기)));

                // 초기 인벤토리 설정 (LoadGame 이후)
                if (inventory == null || inventory.Count == 0)
                {
                    inventory =
                    [
                        // 초기 인벤토리 아이템 추가
                        new Item("운영자의 에테르넬 슈트", StatType.방어력, 5000, "운영자 스태프입니다.", EquipmentType.방어구),
                        new Item("운영자의 데스티니 스태프", StatType.전투력, 5000, "운영자 스태프입니다.", EquipmentType.무기),
                        new Item("나무 스태프", StatType.전투력, 3, "엘리니아 마법사들이 사용하는 흔한 스태프입니다.", EquipmentType.무기)
                    ]; // null 방지용 초기화
                }

                // 게임 흐름을 보여주는 화면
                ShowMainMenu();
                ShowStatus();

                // 게임 종료 전에 저장
                SaveGame();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"예외 발생: {ex.Message}");
            }
            finally
            {
                // 게임 종료 직전 저장
                SaveGame();
                Console.WriteLine("[게임이 저장되었습니다.]");

                // 자동 저장 타이머 종료
                autoSaveTimer?.Dispose();
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

            int totalMaxHP = player.MaxHP + inventory.Where(i => i.IsEquipped && i.Type == StatType.체력).Sum(i => i.Power);

            Console.WriteLine($"'500'메소를 지불하시면 파워엘릭서를 드립니다. (보유 메소: {player.Meso}");

            if (player.Meso >= 500)
            {
                player.HP = Math.Min(player.HP + (totalMaxHP - player.HP), totalMaxHP);
                player.Meso -= 500;

                Console.WriteLine("\n[파워엘릭서의 강력함으로 체력을 전부 회복합니다...]\n");
                Console.WriteLine($"현재 체력: {player.HP} / {totalMaxHP}");
                Console.WriteLine($"남은 메소: {player.Meso}");

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

            if (inventory != null)
            {
                // inventory가 null이 아닌 경우에만 추가적인 계산
                totalAttack += inventory.Where(i => i != null && i.IsEquipped && i.Type == StatType.전투력).Sum(i => i.Power);
                totalDefense += inventory.Where(i => i != null && i.IsEquipped && i.Type == StatType.방어력).Sum(i => i.Power);
                totalMaxHP += inventory.Where(i => i != null && i.IsEquipped && i.Type == StatType.체력).Sum(i => i.Power);
            }
            else
            {
                // inventory가 null인 경우 처리 로직
                Console.WriteLine("Inventory가 null입니다.");
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
            Console.WriteLine($"메소:  {player.Meso} meso\n");

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
            Console.WriteLine("인벤토리\n보유 중인 아이템을 사용할 수 있습니다.\n");
            Console.WriteLine("[아이템 목록]");

            if (inventory.Count == 0)
            {
                Console.WriteLine("(아이템이 없습니다.)\n");
            }
            else
            {
                PrintInventory();
            }

            Console.WriteLine("1. 장착 관리");
            Console.WriteLine("0. 나가기");
            Console.Write("\n>> ");
            string? input = Console.ReadLine();

            if (input == "1")
            {
                ManageEquipment();
            }
            else if (input == "0")
            {
                Console.WriteLine("\n[마을로 돌아갑니다...]\n");
                ShowMainMenu();
            }
            else
            {
                Console.WriteLine("잘못된 입력입니다.\n");
            }
        }

        // 아이템을 장착 관리하는 함수
        static void ManageEquipment()
        {
            Console.WriteLine("인벤토리 - 장착 관리\n 보유 중인 아이템을 관리할 수 있습니다.");
            Console.WriteLine("[아이템 목록]");

            // 장착할 수 있는 아이템만 필터링
            var equipableItems = inventory.Where(i => i.EquipmentType == EquipmentType.무기 ||
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

            Console.WriteLine("0. 나가기\n");
            Console.WriteLine("\n>> ");
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
                        var itemToUpdate = inventory.First(i => i.Name == selectedItem.Name);
                        itemToUpdate.IsEquipped = false;

                        Console.WriteLine($"{selectedItem.Name}을(를) 해제했습니다.");
                        ManageEquipment(); // 장착 관리 화면을 다시 출력
                    }
                    else
                    {
                        // 동일한 아이템 타입을 가진 기존 장착 아이템 해제
                        var equippedItem = inventory.FirstOrDefault(i => i.IsEquipped && i.EquipmentType == selectedItem.EquipmentType);
                        if (equippedItem != null)
                        {
                            equippedItem.IsEquipped = false;
                            Console.WriteLine($"{equippedItem.Name}을(를) 해제하고 {selectedItem.Name}을(를) 장착합니다.");
                        }

                        // 새로운 아이템을 장착
                        selectedItem.IsEquipped = true;

                        // 장착된 아이템을 업데이트
                        var itemToUpdate = inventory.First(i => i.Name == selectedItem.Name);
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
            if (inventory.Count == 0)
            {
                Console.WriteLine("(아이템이 없습니다.)");
                return;
            }

            for (int i = 0; i < inventory.Count; i++)
            {
                Item item = inventory[i];
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
            Console.WriteLine("상점");
            Console.WriteLine("필요한 아이템을 얻을 수 있는 상점입니다.\n");

            Console.WriteLine("[보유 메소]");
            Console.WriteLine($"{player.Meso} 메소\n");

            Console.WriteLine("[아이템 목록]");
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

            Console.WriteLine("\n1. 아이템 구매");
            Console.WriteLine("2. 아이템 판매");
            Console.WriteLine("0. 나가기");
            Console.Write("\n원하시는 행동을 입력해주세요.\n>> ");
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
                Console.WriteLine("\n[아이템 목록]");
                for (int i = 0; i < shopItems.Count; i++)
                {
                    var item = shopItems[i];
                    string status = item.IsPurchased ? "구매완료" : $"{item.Price} 메소";
                    Console.WriteLine($"{i + 1}. {item.ItemData.Name} | {item.ItemData.Type} +{item.ItemData.Power} | {item.ItemData.Description} | | {item.ItemData.EquipmentType}");
                }

                Console.WriteLine("0. 나가기");
                Console.Write("\n원하시는 아이템 번호를 입력해주세요.\n>> ");
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
                        inventory.Add(selectedItem.ItemData);
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
            Console.WriteLine("\n[판매할 아이템 목록]");

            // 판매할 수 있는 아이템만 필터링 (장착되지 않은 아이템들)
            var sellableItems = inventory.Where(i => i.IsEquipped == false).ToList();

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
                // 아이템 가격은 구매가격의 절반으로 책정
                int salePrice = item.Price / 2;
                Console.WriteLine($"{i + 1}. {item.Name} | 판매가격: {salePrice} 메소 | {item.Description}");
            }

            Console.WriteLine("0. 나가기");
            Console.Write("\n판매할 아이템 번호를 입력해주세요.\n>> ");
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

                    // 아이템 판매 가격 계산
                    int salePrice = selectedItem.Power / 2;

                    // 메소 추가
                    player.Meso += salePrice;

                    // 아이템을 인벤토리에서 제거
                    inventory.Remove(selectedItem);

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
            data.Inventory = inventory;
            data.ShopPurchases = shopItems.Select(item => item.IsPurchased).ToList();
            data.HasClassChanged = player.HasClassChanged;  // 전직 여부 저장

            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });

            string filePath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())?.FullName ?? Directory.GetCurrentDirectory(),"savegame.json");
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
                player = data.Player ?? new Character();  // Player가 null일 경우 새로운 Character 객체 생성
                inventory = data.Inventory ?? new List<Item>();  // Inventory가 null일 경우 빈 리스트로 초기화
                                                                 // ShopPurchases를 사용하여 shopItems 생성
                shopItems = new List<ShopItem>();
                for (int i = 0; i < data.ShopPurchases.Count; i++)
                {
                    var item = shopItems[i];  // 여기서 기존 상점 아이템 리스트를 가져와야 할 부분
                    bool isPurchased = data.ShopPurchases[i];
                    item.IsPurchased = isPurchased;  // 아이템의 구매 여부 갱신
                }

                player.Attack = data.Player?.Attack ?? 10;  // 공격력 로드
                player.Defense = data.Player?.Defense ?? 5;  // 방어력 로드
                player.Exp = data.Player?.Exp ?? 0;
                player.Level = data.Player?.Level ?? 1;
                player.HasClassChanged = data.HasClassChanged;  // 전직 여부 불러오기

                // 전직이 완료되었다면 직업을 마법사로 설정
                if (player.HasClassChanged)
                {
                    player.Job = "마법사";
                }
                else
                {
                    // 전직이 완료되지 않았으면 초보자 직업을 유지
                    player.Job = "초보자";
                }
            }
        }

        // 전투 기능 처리 함수
        static void Battle(Monster monster)
        {
            while (player.HP > 0 && monster.HP > 0)
            {
                Console.WriteLine($"{monster.Name}의 체력: {monster.HP}");
                Console.WriteLine($"{player.Name}의 체력: {player.HP}");
                Console.WriteLine("1. 공격\n2. 방어\n0. 도망");
                string? input = Console.ReadLine();

                if (input == "1")  // 공격
                {
                    int totalAttack = player.Attack + inventory.Where(i => i.IsEquipped && i.Type == StatType.전투력).Sum(i => i.Power);
                    int totalDefense = player.Defense + inventory.Where(i => i.IsEquipped && i.Type == StatType.방어력).Sum(i => i.Power);
                    int totalMaxHP = player.MaxHP + inventory.Where(i => i.IsEquipped && i.Type == StatType.체력).Sum(i => i.Power);

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
                        player.HP = (int) (totalMaxHP * 0.5);
                        Console.WriteLine($"{player.Name}은 쓰러졌습니다. 마을로 이송됩니다.\n{player.Name}의 체력이 {player.HP}로 회복되었습니다.");
                        ShowMainMenu();
                        break;
                    }
                }
                else if (input == "0")  // 도망
                {
                    Console.WriteLine("[전투에서 도망쳤습니다.]");
                    Console.WriteLine("\n[마을로 돌아갑니다...]\n");
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

            // 머쉬맘의 드롭 아이템 설정
            if (monsterName.Contains("머쉬맘"))  // 머쉬맘일 경우에만 드롭 아이템 설정
            {
                Random rand = new Random();
                double dropChance = rand.NextDouble();  // 0과 1 사이의 실수값 반환

                // 예를 들어, 20% 확률로 드롭 아이템을 주기로 설정
                if (dropChance <= 0.7)  // 드랍률
                {
                    monster.AddDroppedItem(new Item("머쉬맘의 포자", StatType.체력, 50, "[튜토리얼 보스]머쉬맘의 포자이다. 왠지 머리에 쓰면 든든할 것 같다.", EquipmentType.모자));
                    Console.WriteLine("머쉬맘의 포자가 드랍되었습니다!");
                }
            }

            Battle(monster);  // 전투 시작
        }
    }
}
