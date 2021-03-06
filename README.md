# DynamicDbContext

It will allow you to create a fully dynamic database from complied in-memory assembly with auto migration using Entity framework

Tested with MS SQL, Oracle

#How to Use Sample

         Database.SetInitializer<MyDbContext>(new MigrateDatabaseToLatestVersion<MyDbContext, Configuration>());
            EntityBuilder entityBuilder = new EntityBuilder();
            var Employees = new Model.DynamicEntity
            {
                BaseType
                = typeof(EntityBase),
                Id = 1,
                IsDetailedEntity = false,
                MasterId = 1,
                Name = "Employees",
                TableName = "Employees",
                UniqueId = Guid.NewGuid()
            };
            Employees.Properties.Add(new Model.DynamicProperty
            {
                IsForignKey = false,
                PropertyType = typeof(System.String),
                PrropertyName = "FirstName",
                MaxStringSize = 200
            });
            Employees.Properties.Add(new Model.DynamicProperty
            {
                IsForignKey = false,
                PropertyType = typeof(System.String),
                PrropertyName = "LastName",
                MaxStringSize = 200
            });
            Employees.EntityType = entityBuilder.BuildEntityType(Employees);

            var Addresses = new Model.DynamicEntity
            {
                BaseType = typeof(EntityBase),
                Id = 2,
                IsDetailedEntity = false,
                MasterId = 1,
                Name = "Addresses",
                TableName = "Addresses",
                UniqueId = Guid.NewGuid()
            };
            Addresses.Properties.Add(new Model.DynamicProperty
            {
                PropertyType = typeof(System.String),
                PrropertyName = "City",
                MaxStringSize = 200,
                IsForignKey = false,
            });
            //add  Forign key  and nested  table
            Addresses.Properties.Add(new Model.DynamicProperty
            {
                PropertyType = Employees.EntityType,
                PrropertyName = "Employee",
                IsVirtual = true,
            });

            Addresses.Properties.Add(new Model.DynamicProperty
            {
                PropertyType = typeof(System.Int32),
                PrropertyName = "EmployeeId",
                MaxStringSize = 200,
                IsForignKey = true,
                ForignKey = "Employee"
            });
            Addresses.EntityType = entityBuilder.BuildEntityType(Addresses);
            DynamicAssembly.Entities = new ObservableCollection<Model.DynamicEntity>();
            DynamicAssembly.Entities.Add(Employees);
            DynamicAssembly.Entities.Add(Addresses);
            DynamicAssembly._Context = DbContextFactory.CreateDBContext<MyDbContext>();
            DbContextFactory.UpdateDynamicDataBase<Configuration>(new Configuration());
            var exmployeesEntityType = DynamicAssembly.Entities.SingleOrDefault(x => x.Id == 1).EntityType;
            var employees = DynamicAssembly._Context.Set(exmployeesEntityType);

            dynamic newRecord = (Activator.CreateInstance(exmployeesEntityType));
            newRecord.FirstName = "Ibrahim";
            newRecord.LastName = "Abulubad";
            employees.Add(newRecord);
            DynamicAssembly._Context.SaveChanges();
            List<CustomDynamicExpression> dynamicFilter = new List<CustomDynamicExpression>();
            dynamicFilter.Add(new CustomDynamicExpression
            {
                DynamicCondition = new DynamicCondition
                {
                    Column = "FirstName",
                    Value = "Ibrahim",
                    WhereOperation = DynamicHelper.Enums.WhereOperation.Equal
                }
            }
            );
            var query = employees.Where( dynamicFilter);
 
            var list = query.OrderBy("Id", SortDirection.Asc).Skip(0).Take(10).ToListAsync().GetAwaiter().GetResult();




**Result**

![alt text](/DynamicDbContext/Res/result.png)
 
