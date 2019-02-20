        private IQueryable<PedidoCliente> ApplySorting<PedidoCliente>(IQueryable<PedidoCliente> source,
                                  string sortingParam)
        {
            string paramName = sortingParam.Split(":")[0];
            string sortDirection = sortingParam.Split(":")[1] ?? "asc";
            string command;

            if (sortDirection.Equals("asc"))
                command = "OrderBy";
            else
                command = "OrderByDescending";

            var type = typeof(PedidoCliente);
            var property = type.GetProperty(paramName);
            var parameter = Expression.Parameter(type, "p");
            var propertyAccess = Expression.MakeMemberAccess(parameter, property);
            var orderByExpression = Expression.Lambda(propertyAccess, parameter);
            var resultExpression = Expression.Call(typeof(Queryable), command, new Type[] { type, property.PropertyType },
                                          source.Expression, Expression.Quote(orderByExpression));
            return source.Provider.CreateQuery<PedidoCliente>(resultExpression);
        }