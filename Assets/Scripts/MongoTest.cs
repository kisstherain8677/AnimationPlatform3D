using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;

class MongoTest:MonoBehaviour
{

    private void Start()
    {
        Fun();
    }
    public void Fun()
    {
        /*
         * 所有的动作基于BsonDocument操作就对了
         */
        //数据库连接，格式为mongodb://账号:密码@服务器地址:端口/数据库名
        string conStrMdb = "mongodb://user1:123456@localhost:27017/AnimationSource";
        MongoDbHelper mgDbHelper = new MongoDbHelper(conStrMdb);
        var db = mgDbHelper.GetDb(conStrMdb);
        string colName = "Actor";

        //查询，在这之前是没有数据的，自然也就查不出来数据，我们可以先给它插入一条数据
        var cc = db.GetCollection<Actor_info>(colName);
        var ccList = cc.Find(new BsonDocument()).ToList();
        Console.WriteLine($"【当前集合{colName}数据量】>>>【{ccList.Count}】>>>{DateTime.Now}");
        //建议以模型的方式插入数据，这样子字段类型是可控的
        Actor_info personIndex = new Actor_info
        {
            Url = "testUrl",
            Type = "1-1",
            Animation =null,
            Clothes=null,
            Name="testName"
            };
        //转化为BsonDocument是为了方便移除_id
        BsonDocument document = personIndex.ToBsonDocument();
        document.Remove("_id");
        var result = mgDbHelper.Insert(colName, document);
        Console.WriteLine($"【插入】>>>【{result.iFlg}】>>>{DateTime.Now}");

        //现在我们再查询试下，这里可以添加查询条件
        BsonDocument document1 = new BsonDocument();
        //document1.Add("Id_", personIndex.Id_);
        document1.Add("Name", personIndex.Name);
        var cc1List = cc.Find(document1).ToList();
        for (int i = 0; i < cc1List.Count; i++)
        {
            Console.WriteLine(cc1List[i].ToJson());
        }

        //更新，使用表达式树，实现更新逻辑，注意类型一定要跟字段对应上
        //UpdateDocument update = new UpdateDocument("$set", new BsonDocument() { { "CreateTime", DateTime.Now } });
        //Expression<Func<person_index, bool>> exp = (s => s.Id_ == personIndex.Id_ && s.FullName == personIndex.FullName);
        //var upResult = mgDbHelper.Update(colName, exp, update, false);
        //Console.WriteLine($"【更新】>>>【{upResult.iFlg}】>>>{DateTime.Now}");

        //删除操作的话就不演示了，慎用！
        //Expression<Func<Actor_info, bool>> deleteExp = (s => s.Id_ == personIndex.Id_ && s.FullName == personIndex.FullName && s.CreateTime == personIndex.CreateTime);
        //由于我们更新了日期，这里的删除操作应该是不会生效的
        //var deleteResult = mgDbHelper.Delete(colName, deleteExp);
        //Console.WriteLine($"【删除】>>>【{deleteResult.iFlg}】>>>{DateTime.Now}");
    }

   
}

