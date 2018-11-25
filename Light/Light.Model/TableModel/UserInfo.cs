using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Light.Model.TableModel
{ 
    [Table("UserInfo")]
    public class UserInfo
    {
  [Key]
  public int      UserId                  {get;set;}
  public string  UserName                 {get;set;}
  public string   LoginUserName           {get;set;}
  public string   Pwd                     {get;set;}
  public DateTime   LastLoginTime         {get;set;}
  public string  LastLoginIP              {get;set;}
  public int       DelFlag                {get;set;}
  public DateTime? SubTime                 {get;set;}


    }
}
