﻿using LiteDB;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using bvue.code.Backend;
using WebApplication2Vue;

namespace bVue.code.Backend.repo
{
    public enum OrderBy{
        Desc,Asc
    }
    public class LiteRepo //: Hub
    {
      static string dbpath = @"Data//data.db";
      public LiteRepo()
       {  
           var m = new mapdb();
           var path =Startup.config["dbpath"];
            _db = new LiteDatabase(path!=null?path: dbpath, m.maper);
       
        }

        // static mapper m ;
        public LiteDatabase _db = null;
        public LiteDatabase db
        {
            get
            {
                return _db;
            }
        }



        public async Task<List<TEntity>> GetQuery<TEntity>(Expression<Func<TEntity, bool>> predicate, string orderby, int page, int pageSize = int.MaxValue ) where TEntity : class
        {
     
                if (string.IsNullOrEmpty(orderby))
                { orderby = "ID desc"; }
                OrderBy orderByDir = parseOrderBy(orderby);
                var col = db.GetCollection<TEntity>(GetName(typeof(TEntity)));
                var f = col.Find(predicate);
                if(orderByDir==OrderBy.Desc)
                {
                f = f.OrderByDescending(s => s.GetType()
                        .GetProperty(orderby.Split(' ')[0]).GetValue(s,null));
                }
                else{ f = f.OrderBy(s => s.GetType()
                        .GetProperty(orderby.Split(' ')[0]).GetValue(s,null)); }
                int count = f.Count();
                
                var r = await Task.Run(()=> f.Skip((page - 1) * pageSize).Take(pageSize).ToList()); //., pageSize).ToList();
                r.ForEach(el=>(el as BEntity).Count =count);//Bentity
                return  r;
       
        }

        private OrderBy parseOrderBy(string orderby)
        {
            string t = orderby.ToLower().Trim();
            if(t.EndsWith(" asc"))
            {
               var spl = t.Split(' ');
              return OrderBy.Asc;
            }
            return OrderBy.Desc;
        }

        public int Insert<TEntity>(TEntity data) where TEntity : class
        {           
              //  return await  Task.Run(()=> {
                  
          //      db.Engine.BeginTrans();
                        var col = db.GetCollection<TEntity>(GetName(typeof(TEntity)));
                        var r = col.Insert(data);//db.Com
       //         db.Engine.Commit();
                    return r;
               // db.
                    
            //    });            
        }
        public async Task<bool> Update<TEntity>(TEntity data) where TEntity : class
        {
          
                var col = db.GetCollection<TEntity>(GetName(typeof(TEntity)));
               if(data is BEntity)
               { (data as BEntity).DateChanged = DateTime.Now; }//bentity
                return await Task.Run(()=>   col.Update(data));
        
        }
        public  async Task<int> Delete<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class
        {
        
                var col = db.GetCollection<TEntity>(GetName(typeof(TEntity)));

                return  await Task.Run(()=>col.Delete( predicate));
      
        }



        public static string GetName(Type type)
        {
            var str = (type.Name);
            if (str.Contains("]"))
            { str = str.Substring(str.LastIndexOf(']')); }
            if (str.Contains(","))
            { str = str.Split(',')[0]; }
            if (str.Contains("."))
            {
                str = str.Substring(str.LastIndexOf(".") + 1).ToLower();
            }
            return str.ToLower();
        }

    }
}
