using DynamicDbContext.Builders;
using DynamicDbContext.MemoryStore;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace DynamicDbContext.Factory
{
    public static class DbContextFactory
    {
        private static readonly object syncLock = new object();
        public static T CreateDBContext<T>() where T : class
        {
            Type myType = CompileDbContextType(typeof(T));
            return (T)Activator.CreateInstance(myType);
        }
        public static void UpdateDynamicDataBase<T>(T config) where T : DbMigrationsConfiguration
        {
            lock (syncLock)
            {
                if (DynamicAssembly.HasChanged)
                {
                    config.ContextType = DynamicAssembly._Context.GetType();
                    DbMigrator dbMigrator = new DbMigrator(config);
                    dbMigrator.Update();
                    DynamicAssembly.HasChanged = false;
                    DynamicAssembly.AllowAccessContext = true;
                }
            }
        }


        public static Type CompileDbContextType(Type dbContextType)
        {
            TypeBuilder tb = GetDbConntextTypeBuilder(dbContextType);
            ConstructorBuilder constructor = tb.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);

            // NOTE: assuming your list contains Field objects with fields FieldName(string) and FieldType(Type)
            if (DynamicAssembly.Entities != null)
            {
                //Create Main   Entites
                Type myGeneric = typeof(DbSet<>);
                foreach (Model.DynamicEntity entity in DynamicAssembly.Entities)
                {
                    Type constructedClass = myGeneric.MakeGenericType(entity.EntityType);
                    DynamicPropertyBuilder.CreateProperty(tb, entity.Name, constructedClass);
                }
            }

            Type objectType = tb.CreateType();
            return objectType;
        }
        private static TypeBuilder GetDbConntextTypeBuilder(Type dbContextType)
        {
            string typeSignature = "DynamicContext";
            AssemblyName an = new AssemblyName(typeSignature);
            AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(an, AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("DynamicDbContextMainModule");
            TypeBuilder tb = moduleBuilder.DefineType(typeSignature,
                    TypeAttributes.Public |
                    TypeAttributes.Class |
                    TypeAttributes.AutoClass |
                    TypeAttributes.AnsiClass |
                    TypeAttributes.BeforeFieldInit |
                    TypeAttributes.AutoLayout, dbContextType);
            return tb;
        }
    }
}
