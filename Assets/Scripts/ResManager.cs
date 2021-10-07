using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Bson;

//人物资源类管理类，包含所有的游戏对象，以及各个资源列表的索引值
public class ActorRes
{
    //public string PackageName;//包名
    public string CharacterName;//包内的角色名
    
    public GameObject mSkeleton;//人物骨骼对象rgv

    //显示每个部位的索引,key为部位枚举变量的值，value为某个部位的素材索引号
    public Dictionary<int,int> partIndexDic=new Dictionary<int,int>();

    //存储各个部位资源列表的字典，key为部位枚举变量的值，value为该部位对应的资源列表
    //如果key只有一个元素且为EP_All，说明该角色没有各个部位的素材，只有一个整体素材
    public Dictionary<int, List<GameObject>> assetListDic = new Dictionary<int, List<GameObject>>();
   
    public int mAnimIdx = 0;//动画资源的索引
    public List<AnimationClip> mAnimList = new List<AnimationClip>();//动画资源数组

    

    //重置所有资源
    public void Reset()
    {
       foreach(var key in partIndexDic.Keys)
        {
            partIndexDic[key] = 0;
        }
    }
    public ActorRes()
    {
        foreach (int code in Enum.GetValues(typeof(EPart)))
        {
            partIndexDic.Add(code, 0);
        }

    }

   
}

public enum EPart
{
    EP_Eyes,
    EP_Face,
    EP_Hair,
    EP_Pants,
    EP_Shoes,
    EP_Top,
    EP_All
}
 
public class ResManager : MonoBehaviour
{
    #region 常量

    private const string SkeletonName = "skeleton";

    #endregion

    #region 变量
    
    Dictionary<int, string> partNameDic = new Dictionary<int, string>();//存储各个枚举变量名字字符串的字典
    private List<ActorRes> mActorResList = new List<ActorRes>(); //一个list保存一个角色的资源信息信息
    private ActorRes mActorRes = null;//当前的ActorRes
    private int mActorResIdx = 0;

    private Character mCharacter = null;

    MongoDbHelper mgDbHelper;
    IMongoDatabase database;
    string DbUrl = "mongodb://user1:123456@localhost:27017/AnimationSource";

    public GameObject inputgo;//从数据库中查找到的预制体
    public Actor_info actorinfo;//从数据库中查找到的文档实体

    #endregion

    #region GUI输入变量
    string characterId = "0";
    string partId = "0";
    string assetId = "0";
    string animId = "0";

    public Vector3 startPos;
    public Quaternion startRot;
   
    #endregion

    #region 函数

    /// <summary>
    /// 根据角色名，从数据库中获取该角色的信息，取出该角色的预制体
    /// </summary>
    /// <param name="name"></param>
    void getActorInfoByName(string name)
    {
        

        //保存该角色的资源类
        ActorRes res = new ActorRes();
        //获取collection
        BsonDocument document = new BsonDocument();
        document.Add("Name", name);
        var actorList = mgDbHelper.FindByCondition<Actor_info>("Actor", document).ToList();
        // var ccList = collection.Find(document1).ToList();
        // 如果有重名对象，返回第一个
        if (actorList.Count == 0)
        {
            Debug.Log("数据库中没有此角色名称");
            return;
        }
        actorinfo = actorList[0];
        //根据路径，导入gameobject
        inputgo = Resources.Load<GameObject>(actorinfo.Url);

        //查找动画资源并存储
        foreach(ObjectId animId in actorinfo.Animation)
        {
            document.Clear();
            document.Add("_id", animId);
            var AnimList = mgDbHelper.FindByCondition<Animation_info>("Animation", document).ToList();
            AnimationClip clip = Resources.Load<AnimationClip>(AnimList[0].Url);
            res.mAnimList.Add(clip);
        }
        
        
        //res.mAnimList = actorinfo.Animation;
        res.CharacterName = actorinfo.Name;

        this.mActorResList.Add(res);

       
    }

    /// <summary>
    /// 组装动画
    /// </summary>
    void AssembleAnimation()
    {
        if (inputgo == null)
        {
            Debug.Log("组装动画前要找到gameobject");
            return;
        }
        Animation ani;
        //挂载Character脚本
        if (inputgo.GetComponent<Character>() == null)
        {
            mCharacter = inputgo.AddComponent<Character>();
        }
        else
        {
            mCharacter = inputgo.GetComponent<Character>();
        }
        if (inputgo.GetComponent<Animation>() == null)
        {
            ani = inputgo.AddComponent<Animation>();
        }
        else
        {
            ani = inputgo.GetComponent<Animation>();
        }

        //添加Animation组件，以及该角色的动画列表
        foreach (ActorRes res in this.mActorResList)
        {
            if (res.CharacterName == inputgo.name)
            {
                foreach (AnimationClip aniclip in res.mAnimList)
                {

                    if (ani.GetClip(aniclip.name) == null)
                    {
                        aniclip.legacy = true;
                        ani.AddClip(aniclip, aniclip.name);
                    }
                    else
                    {
                        ani.GetClip(aniclip.name).legacy = true;
                    }

                }
                break;
            }
        }
        if (ani.GetClipCount() == 0)
        {
            Debug.LogError("为该角色添加动画列表失败。");
        }

    }

    /// <summary>
    /// 根据预制体、出生点等信息，调用Character组件，
    /// 根据指定的信息和预制体生成游戏物体。
    /// </summary>
    /// <param name="name"></param>
    /// <param name="inputgo"></param>
    /// <param name="startPosition"></param>
    /// <param name="startRotation"></param>
    /// <param name="suitable"></param>
    /// <returns></returns>
    public GameObject InitCharacter(string name,Vector3 startPosition,Quaternion startRotation,bool suitable)
    {
        if (name == "")
        {
            Debug.Log("未指定角色名字！");
            return null;
        }
        
        getActorInfoByName(name);//从数据库获取角色信息

        AssembleAnimation();//组装动画

        if (suitable)
        {
            //inputGo = new GameObject();
            //mCharacter = inputgo.AddComponent<Character>();
            //mCharacter = GameObject.Instantiate(this.mCharacter);
            //mCharacter = new Character();
           

            //mCharacter = inputgo.AddComponent<Character>();//在Character对象上添加脚本组件
            mCharacter.SetName(name);

            mActorRes = mActorResList[mActorResIdx];
            //生成各个部位的预制体
            mCharacter.Generate(mActorRes,inputgo,startPosition,startRotation);//根据索引值生成各个部位
            
        }
        //GameObject go = new GameObject();//生成对象Character
        else
        {
            
            //mCharacter = inputgo.AddComponent<Character>();
            //mCharacter = GameObject.Instantiate(this.mCharacter);
            //mCharacter = new Character();
            mCharacter.Generate(inputgo, startPosition, startRotation);
            
        }
        return mCharacter.getGameObject();//返回实例化后的游戏物体Character.genGo

    }

    public void ChangeClothes(int characterIndex, int partIndex, int assetIndex)
    {
        ActorRes ActorRes = mActorResList[characterIndex];
        if (characterIndex > mActorResList.Count - 1)
        {
            Debug.Log("character index out of range!");
            return;
        }
        if (partIndex > ActorRes.partIndexDic.Count - 1)
        {
            Debug.Log("part index out of range!");
            return;
        }
        if (assetIndex > ActorRes.assetListDic[partIndex].Count - 1)
        {
            Debug.Log("assetIndex index out of range!");
            return;
        }

        //切换某部位的服装index
        ActorRes.partIndexDic[partIndex] = assetIndex;
        mActorResList[characterIndex] = ActorRes;
        mCharacter.Generate(mActorResList[characterIndex],inputgo,mCharacter.position,mCharacter.rotation);

    }

    
    public void ChangeAnim(int characterIndex, int animIndex)
    {
        ActorRes ActorRes = mActorResList[characterIndex];
        if (animIndex > ActorRes.mAnimList.Count - 1)
        {
            Debug.Log("animIndex out of range");
            return;
        }
        ActorRes.mAnimIdx = animIndex;
        mActorResList[characterIndex] = ActorRes;
        mCharacter.Generate(mActorResList[characterIndex],inputgo,mCharacter.position,mCharacter.rotation);
    }

    private void InitPartName()
    {
        partNameDic.Add(0, "eyes");
        partNameDic.Add(1, "face");
        partNameDic.Add(2, "hair");
        partNameDic.Add(3, "pants");
        partNameDic.Add(4, "shoes");
        partNameDic.Add(5, "top");

    }

    /// <summary>
    /// 读取资源文件，创建所有的人物素材
    /// mActorResList中存放包内的所有角色资源文件
    /// 每个ActorRes中有每个部位的所有游戏物体，以及动画
    /// </summary>
    public void LoadPackageAssets(string packageName)
    {
        InitPartName();

        string path = "Characters/" + packageName + "/";
        //string ChaPathName = "Characters/TestClothes/Prefabs";
        //string AniPathName = "Animations/TestClothes/";
        //string AniPathName = "Characters/TestClothes/";  
        DirectoryInfo dir = new DirectoryInfo("Assets/Resources/" + path);
        foreach (var subdir in dir.GetDirectories())//得到包内各个角色的资源目录
        {
            string[] splits = subdir.Name.Split('/');
            string dirname = splits[splits.Length - 1];

            GameObject[] golist = Resources.LoadAll<GameObject>(path + dirname + "/Prefabs");//导入所有的prefab

            ActorRes ActorRes = new ActorRes();
            
            mActorResList.Add(ActorRes);

            ActorRes.CharacterName = dirname;
            if (golist.Length == 1)//没有各个部位的预制体，载入一个预制体到作为All这个key的值
            {
                ActorRes.assetListDic[(int)EPart.EP_All] = golist.ToList<GameObject>();
            }
            else//否则找到所有预制体，根据名字查找各个部位的资源
            {
                //找骨骼文件
                ActorRes.mSkeleton = FindRes(golist, SkeletonName)[0];

                foreach (int key in partNameDic.Keys)
                {
                    ActorRes.assetListDic[key] = FindRes(golist, partNameDic[key]);
                }
            }

            //找所有的动画文件
            AnimationClip[] clips = Resources.LoadAll<AnimationClip>(path + dirname + "/Animations");
            //List<AnimationClip> clips = FunctionUtil.CollectAll<AnimationClip>(animpath);
            ActorRes.mAnimList.AddRange(clips);
        }
    }

    /// <summary>
    /// 根据字符串在所有的gameobject中查找相应的资源
    /// </summary>
    private List<GameObject> FindRes(GameObject[] golist, string findname)
    {
        List<GameObject> findlist = new List<GameObject>();
        foreach (var go in golist)
        {
            if (go.name.Contains(findname))
            {
                findlist.Add(go);
            }
        }

        return findlist;
    }

    private void InitDatabase()
    {
        
        mgDbHelper = new MongoDbHelper(DbUrl);
        database = mgDbHelper.GetDb(DbUrl);
    }

    #endregion

    #region 内置函数

    // Use this for initialization
    void Start ()
    {
        //素材准备
        //LoadPackageAssets("TestClothes");
        //InitCharacter("Character", inputGo, startPos, startRot, true);//创建角色
        //ChangeClothes(0, 5, 2);//换服装接口,角色id、部位id、该部位服装id
        //ChangeAnim(0, 3);//更换动画接口，角色id、动画id

        //数据库连接
        InitDatabase();

    }
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    /// <summary>
    /// 切换当前角色的服装
    /// </summary>
    /// <param name="characterIndex"> 角色的索引值</param>
    /// <param name="partIndex"> 部位枚举变量的值</param>
    /// <param name="assetInsex"> 某个部位的资源索引值</param>
   

    /*
    private void OnGUI()
    {
        

        GUILayout.BeginArea(new Rect(10, 10, 100, 500));
        GUILayout.BeginVertical();
        GUILayout.Label("输入角色id(0/1)");
        characterId = GUILayout.TextField(characterId);


        GUILayout.Label("输入角色部位id(0-5)");
        GUILayout.Label("0:eyes 1:face 2:hair 3:pants 4:shoes 5:top");
        partId = GUILayout.TextField(partId);
        
        GUILayout.Label("输入该部位的服装资源id");
        assetId = GUILayout.TextField(assetId);
        

        GUILayout.Label("输入动画id");
        animId = GUILayout.TextField(animId);

        
        if (GUILayout.Button("ok"))
        {
            if(characterId==""|| partId == "" || assetId == "")
            {
                Debug.Log("输入不能为空！");
                return;
            }
            ChangeClothes(int.Parse(characterId), int.Parse(partId), int.Parse(assetId));//换服装接口,角色id、部位id、该部位服装id
            ChangeAnim(int.Parse(characterId), int.Parse(animId));//更换动画接口，角色id、动画id
        }
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
    */


    /*
    private void OnGUI()
    {
        GUI.skin.box.fontSize = 20;
        GUI.skin.button.fontSize = 20;

        GUILayout.BeginArea(new Rect(10, 10, typeWidth + 2 * buttonWidth + 8, 1000));

        // Buttons for changing the active character.
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("<", GUILayout.Width(buttonWidth), GUILayout.Height(typeheight)))
        {
            ReduceActorRes();
            mCharacter.Generate(mActorRes);
        }

        GUILayout.Box("Character", GUILayout.Width(typeWidth), GUILayout.Height(typeheight));

        if (GUILayout.Button(">", GUILayout.Width(buttonWidth), GUILayout.Height(typeheight)))
        {
            AddActorRes();
            mCharacter.Generate(mActorRes);
        }

        GUILayout.EndHorizontal();

        // Buttons for changing character elements.
        AddCategory((int)EPart.EP_Face, "Head", null);
        AddCategory((int)EPart.EP_Eyes, "Eyes", null);
        AddCategory((int)EPart.EP_Hair, "Hair", null);
        AddCategory((int)EPart.EP_Top, "Body", "item_shirt");
        AddCategory((int)EPart.EP_Pants, "Legs", "item_pants");
        AddCategory((int)EPart.EP_Shoes, "Feet", "item_boots");

        // anim
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("<", GUILayout.Width(buttonWidth), GUILayout.Height(typeheight)))
        {
            mActorRes.ReduceAnimIdx();
            mCharacter.ChangeAnim(mActorRes);
        }

        GUILayout.Box("Anim", GUILayout.Width(typeWidth), GUILayout.Height(typeheight));

        if (GUILayout.Button(">", GUILayout.Width(buttonWidth), GUILayout.Height(typeheight)))
        {
            mActorRes.AddAnimIdx();
            mCharacter.ChangeAnim(mActorRes);
        }

        GUILayout.EndHorizontal();

        GUILayout.EndArea();
    }

    // Draws buttons for configuring a specific category of items, like pants or shoes.
    void AddCategory(int parttype, string displayName, string anim)
    {
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("<", GUILayout.Width(buttonWidth), GUILayout.Height(typeheight)))
        {
            mActorRes.ReduceIndex(parttype);//step1 改变资源索引值
            mCharacter.ChangeEquip(parttype, mActorRes);//step2 根据索引值更改服装
            
        }

        GUILayout.Box(displayName, GUILayout.Width(typeWidth), GUILayout.Height(typeheight));

        if (GUILayout.Button(">", GUILayout.Width(buttonWidth), GUILayout.Height(typeheight)))
        {
            mActorRes.AddIndex(parttype);
            mCharacter.ChangeEquip(parttype, mActorRes);
                    
        }

        GUILayout.EndHorizontal();
    }

    #endregion
    

    #region 函数
    */

   

    #endregion
}


/// <summary>
/// Actor类
/// </summary>
public class Actor_info
{
    /// <summary>
    /// 系统自带_id
    /// </summary>
    public ObjectId _id { get; set; }
    public string Url { get; set; }
    public string Type { get; set; }
    public ObjectId[] Animation { get; set; }
    public string[][] Clothes { get; set; }
    public string Name { get; set; }
}

/// <summary>
/// Animation类
/// </summary>
public class Animation_info
{
    /// <summary>
    /// 系统自带_id
    /// </summary>
    public ObjectId _id { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public ObjectId[] ActorList { get; set; }
    public double Length { get; set; }
    public string Url { get; set; }
}