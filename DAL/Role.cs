using System;
using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;

namespace DAL;
[BsonCollection("Roles")]
public class Role : MongoIdentityRole<Guid>
{
    
}