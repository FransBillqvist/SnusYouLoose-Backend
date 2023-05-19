using System;
using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;

namespace DAL;
[BsonCollection("AuthUser")]
public class AuthUser : MongoIdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;

}