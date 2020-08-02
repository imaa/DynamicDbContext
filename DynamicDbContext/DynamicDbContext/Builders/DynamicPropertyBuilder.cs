using DynamicDbContext.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace DynamicDbContext.Builders
{
    internal static class DynamicPropertyBuilder
    {
        public static void CreateProperty(TypeBuilder tb, DynamicProperty property)
        {
            FieldBuilder fieldBuilder = tb.DefineField("_" + property.PrropertyName, property.PropertyType, FieldAttributes.Private);

            _CreateProperty(tb, property.PrropertyName, property.PropertyType, fieldBuilder, property);
        }
        public static void CreateProperty(TypeBuilder tb, string propertyName, Type propertyType, DynamicProperty property = null)
        {
            FieldBuilder fieldBuilder = tb.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);

            _CreateProperty(tb, propertyName, propertyType, fieldBuilder, property);
        }
        private static void _CreateProperty(TypeBuilder tb, string prropertyName, Type propType, FieldBuilder fieldBuilder, DynamicProperty property = null)
        {
            PropertyBuilder propertyBuilder = tb.DefineProperty(prropertyName, PropertyAttributes.HasDefault, propType, null);
            MethodAttributes methodAttributes = MethodAttributes.Public
                | MethodAttributes.SpecialName
                | MethodAttributes.HideBySig;
            if (property != null && property.IsVirtual)
            {
                _ = methodAttributes | MethodAttributes.Virtual;
            }
            MethodBuilder getPropMthdBldr = tb.DefineMethod("get_" + prropertyName, methodAttributes, propType, Type.EmptyTypes);
            ILGenerator getIl = getPropMthdBldr.GetILGenerator();
            if (property != null && property.IsForignKey)
            {
                Type attr = typeof(ForeignKeyAttribute);
                ConstructorInfo myConstructorInfo = attr.GetConstructor(new Type[] { typeof(string) });
                CustomAttributeBuilder attrBuilder = new CustomAttributeBuilder(myConstructorInfo, new object[] { property.ForignKey });
                propertyBuilder.SetCustomAttribute(attrBuilder);
            }
            if (property != null && property.PropertyType == typeof(string))
            {
                Type attr = typeof(MaxLengthAttribute);
                ConstructorInfo myConstructorInfo = attr.GetConstructor(new Type[] { typeof(int) });
                CustomAttributeBuilder attrBuilder = new CustomAttributeBuilder(myConstructorInfo, new object[] { property.MaxStringSize });
                propertyBuilder.SetCustomAttribute(attrBuilder);
            }


            getIl.Emit(OpCodes.Ldarg_0);
            getIl.Emit(OpCodes.Ldfld, fieldBuilder);
            getIl.Emit(OpCodes.Ret);

            MethodBuilder setPropMthdBldr =
                tb.DefineMethod("set_" + prropertyName, methodAttributes,
                  null, new[] { propType });

            ILGenerator setIl = setPropMthdBldr.GetILGenerator();
            Label modifyProperty = setIl.DefineLabel();
            Label exitSet = setIl.DefineLabel();

            setIl.MarkLabel(modifyProperty);
            setIl.Emit(OpCodes.Ldarg_0);
            setIl.Emit(OpCodes.Ldarg_1);
            setIl.Emit(OpCodes.Stfld, fieldBuilder);

            setIl.Emit(OpCodes.Nop);
            setIl.MarkLabel(exitSet);
            setIl.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getPropMthdBldr);
            propertyBuilder.SetSetMethod(setPropMthdBldr);
        }


    }
}
