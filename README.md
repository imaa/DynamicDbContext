# DynamicDbContext


#How to Use Sample

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
            PropertyInfo propInfo = DynamicAssembly._Context.GetType().GetProperties().SingleOrDefault(x => x.Name == exmployeesEntityType.Name);
            var query = employees.Where(DynamicAssembly._Context, propInfo, dynamicFilter);
            var list = query.OrderBy("Id", SortDirection.Asc).Skip(0).Take(10).ToListAsync().GetAwaiter().GetResult();
