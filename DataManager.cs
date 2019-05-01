using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
public class DataManager : MonoBehaviour
{
    public static DataManager Instance;
    public List<Image> SelectCharacterImage = new List<Image>();
    private List<CharacterInfo> listCharater = new List<CharacterInfo>();
    public List<int> listParty = new List<int>();
    private List<EnemyInfo> listEnemy = new List<EnemyInfo>();

    public delegate void StageHandler();
    public static event StageHandler StageUp;

    public delegate void CoinHandler();
    public static event CoinHandler CoinUp;

    private SaveData savedata;

    const float DEFAULT_SKILL_MULTIPLE = 1;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }


    [SerializeField]
    public class SaveData
    {
        public int stage { get; set; }

        public int coin { get; set; }

        public int returncoin { get; set; }

        public int GetNeedCoin(int upgrade)
        {
            return upgrade * 2;
        }
        
        public void AddStageNum()
        {
            stage++;
            UpdateStageUI();
        }

        public void MinusStageNum()
        {
            stage--;
            UpdateStageUI();
        }

        public void InitializeStageNum()
        {
            stage = 1;
        }

        public void AddCoin(int coinNum)
        {
            coin += coinNum;
            UpdateCoinUI();
        }

        public void MinusCoin(int coinNum)
        {

            coin -= coinNum;
            if (coin < 0)
                coin = 0;
            UpdateCoinUI();
        }

        public void AddReturnCoin(int coinNum)
        {
            returncoin += coinNum;
        }

        public void MinusReturnCoin(int coinNum)
        {
            if (returncoin < 0)
                returncoin = 0;
            returncoin -= coinNum;
        }

        public void UpdateCoinUI()
        {
            if (CoinUp != null)
                CoinUp();
        }

        public void UpdateStageUI()
        {
            if (StageUp != null)
                StageUp();
        }

        public void ResetConnectUI() //환생후 UI새로만들때 이벤트 초기화
        {
            StageUp = null;
            CoinUp = null;
        }
    }


    public void SetSavedata(SaveData savedataload)
    {
        savedata = savedataload;
    }

    public SaveData GetSavedate()
    {
        return savedata;
    }

    public Text ReturnStageText(Text text)
    {
        text.text = GetSavedate().stage.ToString();
        return text;
    }

    public Text GetCoinText(Text text)
    {
        text.text = string.Format("{0}", GetSavedate().coin);

        return text;
    }

    public Text GetReturnCoinText(Text text)
    {
        text.text = string.Format("{0}", GetSavedate().returncoin);

        return text;
    }
    
    public class EnemyInfo
    {
        public EnemyInfo(int enemykey)
        {
            enemyRow = EnemyTable.Instance.Get(enemykey);
        }

        //public int stage;

        public EnemyTable.Row enemyRow;

    }

    List<SaveCharacterData.CharacterData> TransformData()
    {
        List<SaveCharacterData.CharacterData> characterData = new List<SaveCharacterData.CharacterData>();
        
        for (int i = 0; i < 4; i++)
        {
            List<SaveCharacterData.WeaponData> weaponList = new List<SaveCharacterData.WeaponData>();
            // 게임을 로드했을 때
            for (int j = 0; j < 10; j++)
            {
                weaponList.Add(new SaveCharacterData.WeaponData() //해당캐릭터의 몇번 무기를알아야함
                {
                    //key = listCharater[i].weapon[j].weaponKey,
                    level = listCharater[i].weapon[j].weaponlevel,
                    lockedState = listCharater[i].weapon[j].lockedState,
                    count = listCharater[i].weapon[j].count,
                });
            }

            List<SaveCharacterData.SkillData> skillList = new List<SaveCharacterData.SkillData>();

            for (int j = 0; j < listCharater[i].skill.Count; j++)
            {
                skillList.Add(new SaveCharacterData.SkillData()
                {
                    key = listCharater[i].skill[j].key,
                    level = listCharater[i].skill[j].skillLevel,
                });
            }

            characterData.Add(new SaveCharacterData.CharacterData()
            {
                enchantLevel = listCharater[i].EnchantLevel,

                weaponList = weaponList,
                skillList = skillList,
                equipWeaponKey = listCharater[i].equipWeaponKey,
                enterCharacterBool = listCharater[i].enterCharacterBool,

                //lockedState = true,
            });
        }

        return characterData;
    }

    public void SaveJsonData()
    {
        string commonJsonPath = Application.persistentDataPath + "/CommonData.json";
        string CharacterJsonPath = Application.persistentDataPath + "/CharacterData.json";

        string commonJson = JsonUtility.ToJson(savedata, prettyPrint: true);
        File.WriteAllText(commonJsonPath, commonJson);

        string CharacterJson = JsonHelper.ToJson(TransformData(), prettyPrint: true);
        File.WriteAllText(CharacterJsonPath, CharacterJson);
    }


    public class CharacterInfo
    {
        public CharacterInfo(int characterKey)
        {
            characterRow = CharacterTable.Instance.Get(characterKey);
        }

        public int level;
        public int EnchantLevel;//

        public List<Weapon> weapon = new List<Weapon>();
        public List<Skill> skill = new List<Skill>();
        
        public CharacterTable.Row characterRow;
        public int equipWeaponKey { get; set; }
        public bool enterCharacterBool;

        public void SetEquipWeaponKey(int Weaponkey)
        {
            equipWeaponKey = Weaponkey;
        }

        public int GetEquipWeaponKey()
        {
            return equipWeaponKey;
        }

        public void SetEnterCharacter()
        {
            enterCharacterBool = true;
        }

        public void AddEnchantLevel()
        {
            EnchantLevel++;
            // 할 일
            // UI 
            // 캐릭터 쪽에도 전투중에 사용하는 스텟 값들을 다시 업데이트 시켜줘야 함.
            GetTotalStats();
        }

        public void InititializeCharacter()
        {
            EnchantLevel = 1;
            enterCharacterBool = false;
        }

        public Weapon GetWeapon(int weaponKey)
        {
            foreach (var w in weapon)
            {
                if (w.rowWeapon.key == weaponKey)
                    return w;
            }
            return null;
        }

        // 무기의 총합 스탯을 주세요.
        public TotalCollectionWeaponStat GetTotalCollectionWeapon()
        {
            TotalCollectionWeaponStat totalCollectionWeaponStat = new TotalCollectionWeaponStat();

            for (int i = 0; i < weapon.Count; i++)
            {
                if (weapon[i].lockedState)
                {
                    CalculationCollectWeaponStat(totalCollectionWeaponStat, weapon[i].rowWeapon.cstatType1, 0, i);
                    CalculationCollectWeaponStat(totalCollectionWeaponStat, weapon[i].rowWeapon.cstatType2, 1, i);
                }
            }
            return totalCollectionWeaponStat;
        }


        public TotalSkillStat GetTotalSkillStat()
        {
            TotalSkillStat totalSkillStat = new TotalSkillStat();
            for (int i = 0; i < skill.Count; i++)
            {
                CalculationSkillStat(totalSkillStat, skill[i].rowSkill.valueType, i);
            }

            return totalSkillStat;
        }

        private void CalculationSkillStat(TotalSkillStat totalSkillStat, string type, int index)
        {
            switch (type)
            {
                case "Deffence":
                    totalSkillStat.defMultiple += DEFAULT_SKILL_MULTIPLE + (skill[index].rowSkill.value * skill[index].skillLevel);
                    break;
                case "Attack":
                    totalSkillStat.atkMultiple += DEFAULT_SKILL_MULTIPLE + (skill[index].rowSkill.value * skill[index].skillLevel);
                    break;
                case "Hp":
                    totalSkillStat.hpMultiple += DEFAULT_SKILL_MULTIPLE + (skill[index].rowSkill.value * skill[index].skillLevel);
                    break;
                case "Dmg":
                    totalSkillStat.skillValueMultiple = DEFAULT_SKILL_MULTIPLE + (skill[index].rowSkill.value * skill[index].skillLevel);
                    break;
            }
        }

        public TotalStat GetTotalStats()
        {
            TotalStat totalStat = new TotalStat();

            TotalWeaponStat totalWeaponStat = GetTotlaWEaponStat();
            TotalSkillStat totalSkillStat = GetTotalSkillStat();
            TotalCollectionWeaponStat totalCollectionWeaponStat = GetTotalCollectionWeapon();
            totalStat.atk = (int)((characterRow.atk + totalWeaponStat.atk + EnchantLevel) * totalSkillStat.atkMultiple + totalCollectionWeaponStat.atk);

            totalStat.def = (int)((characterRow.def + totalWeaponStat.def + EnchantLevel) * totalSkillStat.defMultiple + totalCollectionWeaponStat.def);

            totalStat.hp = (int)((characterRow.hp + totalWeaponStat.hp + EnchantLevel) * totalSkillStat.hpMultiple + totalCollectionWeaponStat.hp);

            totalStat.accuracyRate = 90 + totalCollectionWeaponStat.accuracyRate;

            totalStat.avoidanceRate = 10 + totalCollectionWeaponStat.avoidanceRate;


            totalStat.skillValue = totalSkillStat.skillValueMultiple;

            return totalStat;
        }

        public TotalWeaponStat GetTotlaWEaponStat()
        {
            TotalWeaponStat totalWeaponStat = new TotalWeaponStat();

            CalculationStat(totalWeaponStat, weapon[equipWeaponKey].rowWeapon.statType1, 0);
            CalculationStat(totalWeaponStat, weapon[equipWeaponKey].rowWeapon.statType2, 1);
            CalculationStat(totalWeaponStat, weapon[equipWeaponKey].rowWeapon.statType3, 2);
            CalculationStat(totalWeaponStat, weapon[equipWeaponKey].rowWeapon.statType4, 3);

            return totalWeaponStat;
        }

        private void CalculationStat(TotalWeaponStat totalWeaponStat, string type, int index)
        {
            switch (type)
            {
                case "Deffence":
                    totalWeaponStat.def = totalWeaponStat.def + weapon[equipWeaponKey].GetStatList()[index].value;
                    break;
                case "Attack":
                    totalWeaponStat.atk = totalWeaponStat.atk + weapon[equipWeaponKey].GetStatList()[index].value;
                    break;
                case "Hp":
                    totalWeaponStat.hp = totalWeaponStat.hp + weapon[equipWeaponKey].GetStatList()[index].value;
                    break;
            }
        }

        private void CalculationCollectWeaponStat(TotalCollectionWeaponStat totalCollectionWeaponStat, string type, int index, int weaponindex)
        {
            switch (type)
            {
                case "Deffence":
                    totalCollectionWeaponStat.def = totalCollectionWeaponStat.def + weapon[weaponindex].GetCollectionWeaponStatList()[index].cValue;

                    break;
                case "Attack":
                    totalCollectionWeaponStat.atk = totalCollectionWeaponStat.atk + weapon[weaponindex].GetCollectionWeaponStatList()[index].cValue;

                    break;
                case "Hp":
                    totalCollectionWeaponStat.hp = totalCollectionWeaponStat.hp + weapon[weaponindex].GetCollectionWeaponStatList()[index].cValue;

                    break;
                case "AccuracyRate":
                    totalCollectionWeaponStat.accuracyRate = totalCollectionWeaponStat.accuracyRate + weapon[weaponindex].GetCollectionWeaponStatList()[index].cValue;

                    break;
                case "AvoidanceRate":
                    totalCollectionWeaponStat.avoidanceRate = totalCollectionWeaponStat.avoidanceRate + weapon[weaponindex].GetCollectionWeaponStatList()[index].cValue;

                    break;

            }
        }
    }

    public struct TotalWeaponStat
    {
        public int atk;
        public int def;
        public int hp;
    }

    public struct TotalCollectionWeaponStat
    {
        public int atk;
        public int def;
        public int hp;
        public int accuracyRate;
        public int avoidanceRate;
    }

    public struct TotalSkillStat
    {
        public float atkMultiple;
        public float defMultiple;
        public float hpMultiple;

        public float skillValueMultiple;
    }

    public struct TotalStat
    {
        public int atk;
        public int def;
        public int hp;
        public int accuracyRate;
        public int avoidanceRate;

        public float skillValue;
    }

    public class Weapon
    {
        //public int weaponKey;

        public int weaponlevel = 1;
        public int count = 0;

        int selectedStatus;
        public bool lockedState = false;
        public bool newState = false;
        public WeaponTable.Row rowWeapon;

        public Weapon(int weaponKey)
        {
            rowWeapon = WeaponTable.Instance.Get(weaponKey);
        }

        public void AddEnchantWeaponLevel()
        {
            weaponlevel++;
        }

        public class Stat
        {
            public enum eStatType// 공격력, 방어력
            {
                Attack,
                Deffence,
                Hp,
            }

            public enum eCollectionStatType// 공격력, 방어력
            {
                Attack,
                Deffence,
                Hp,
                AccuracyRate,
                AvoidanceRate,
            }
            
            public eStatType statType;
            public eCollectionStatType collectionStatType;
            public int value;
            public int cValue;
        }

        public List<Stat> GetStatList()  //현재 무기 1개의 스탯
        {

            List<Stat> result = new List<Stat>();

            Stat.eStatType type1 = (Stat.eStatType)Enum.Parse(typeof(Stat.eStatType), rowWeapon.statType1, false);
            Stat.eStatType type2 = (Stat.eStatType)Enum.Parse(typeof(Stat.eStatType), rowWeapon.statType2, false);
            Stat.eStatType type3 = (Stat.eStatType)Enum.Parse(typeof(Stat.eStatType), rowWeapon.statType3, false);
            Stat.eStatType type4 = (Stat.eStatType)Enum.Parse(typeof(Stat.eStatType), rowWeapon.statType4, false);

            #region 공식이 다를 경우 스위치케이스로 구분
            //공식이 다를 경우 스위치케이스로 구분

            //switch (type)
            //{
            //    case Status.eStatusType.Attack:
            //        new Status().value = rowWeapon.Value1 * weaponlevel * 2;
            //        break;
            //    case Status.eStatusType.Deffence:
            //        new Status().value = rowWeapon.Value2 * weaponlevel * 2;
            //        break;
            //    case Status.eStatusType.Hp:
            //        new Status().value = rowWeapon.Value3 * weaponlevel * 2;
            //        break;
            //}
            #endregion

            result.Add(new Stat()
            {
                statType = type1,
                value = rowWeapon.Value1 * weaponlevel,
            });

            result.Add(new Stat()
            {
                statType = type2,
                value = rowWeapon.Value2 * weaponlevel,
            });

            result.Add(new Stat()
            {
                statType = type3,
                value = rowWeapon.Value3 * weaponlevel,
            });

            result.Add(new Stat()
            {
                statType = type4,
                value = rowWeapon.Value4 * weaponlevel,
            });
            return result;
        }

        public List<Stat> GetCollectionWeaponStatList()
        {
            List<Stat> result = new List<Stat>();

            Stat.eCollectionStatType type1 = (Stat.eCollectionStatType)Enum.Parse(typeof(Stat.eCollectionStatType), rowWeapon.cstatType1, false);
            Stat.eCollectionStatType type2 = (Stat.eCollectionStatType)Enum.Parse(typeof(Stat.eCollectionStatType), rowWeapon.cstatType2, false);


            result.Add(new Stat()
            {
                collectionStatType = type1,
                cValue = rowWeapon.cValue1 * weaponlevel,
            });

            result.Add(new Stat()
            {
                collectionStatType = type2,
                cValue = rowWeapon.cValue2 * weaponlevel,
            });
            return result;
        }
    }  //무기정보

    public class Skill
    {
        public int key;
        public SkillTable.Row rowSkill;

        public int skillLevel;


        public Skill(int skillkey)
        {
            rowSkill = SkillTable.Instance.Get(skillkey);
            key = skillkey;
        }

        public void skillLevelUp()
        {
            skillLevel++;
        }
    }

    public void ClearCharacter()
    {
        listCharater.Clear();
    }
    public void AddCharacterInfo(CharacterInfo c)
    {
        listCharater.Add(c);
    }

    public CharacterInfo GetCharacter(int key)
    {
        return listCharater[key];
    }

    public int GetCharacterCount()
    {
        return listCharater.Count;
    }

    //Enemy 

    public void AddEnemyInfo(EnemyInfo e)
    {
        listEnemy.Add(e);
    }

    public void ClearEnemy()
    {
        listEnemy.Clear();
    }

    public EnemyInfo GetEnemy(int enemyKey)
    {
        return listEnemy[enemyKey];
    }
}
