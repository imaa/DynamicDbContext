using DynamicDbContext.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace DynamicDbContext.Builders
{
    internal class EntityBuilder
    {
        public object CreateObject(Type entityType)
        {
            return Activator.CreateInstance(entityType);
        }
        public object CreateInstance(DynamicEntity entity)
        {
            Type entityType = BuildEntityType(entity);
            return Activator.CreateInstance(entityType);
        }
        public Type BuildEntityType(DynamicEntity entity)
        {
            TypeBuilder tb = GetTypeBuilder(entity);
            ConstructorBuilder constructor = tb.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);

            foreach (DynamicProperty field in entity.Properties)
            {
                DynamicPropertyBuilder.CreateProperty(tb, field);
            }

            Type objectType = tb.CreateType();
            return objectType;
        }

        private TypeBuilder GetTypeBuilder(DynamicEntity entity)
        {
            string typeSignature = entity.Name;
            AssemblyName an = new AssemblyName(typeSignature);
            AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(an, AssemblyBuilderAccess.Run);

            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");

            TypeBuilder tb = moduleBuilder.DefineType(typeSignature,
                    TypeAttributes.Public |
                    TypeAttributes.Class |
                    TypeAttributes.AutoClass |
                    TypeAttributes.AnsiClass |
                    TypeAttributes.BeforeFieldInit |
                    TypeAttributes.AutoLayout,
                  entity.BaseType);
            return tb;
        }
    }
}
