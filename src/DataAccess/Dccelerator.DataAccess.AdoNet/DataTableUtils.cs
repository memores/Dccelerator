#if (NET40 || NET45)
 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;


namespace Dccelerator.DataAccess.Ado {
    public static class DataTableUtils {

        /// <summary>
        /// ������� <see cref="DataTable"/> �� ���������. � �������� ������� ������������ ���������� ��������, ���� ��� ��������� ���� �� ���� �� ��������.
        /// ������ ���������: ��� ��������� <see cref="DataTable"/> ������������ ���������.
        /// </summary>
        public static DataTable ToDataTable<T>(this IEnumerable<T> collection, params Expression<Func<T, object>>[] expressions) {
            var type = typeof (T);
            PropertyInfo[] props;

            if (expressions.Any()) {
                var expFields = expressions.MemberExpressions().Select(x => x.Member.Name).ToArray();
                var allprops = type.GetProperties().ToArray();
                props = expFields.Select(x => allprops.SingleOrDefault(z => z.Name == x)).Where(x => x != null).ToArray();
            }
            else {
                props = type.GetProperties().Where(x => x.CanRead).ToArray();
            }

            var tableName = type.Name;
            var lastChar = tableName.Last();
            tableName += char.ToLowerInvariant(lastChar).Equals('s') ? "es" : "s";

            var table = new DataTable(tableName);
            table.Columns.AddRange(props.Select(x => new DataColumn(x.Name, Nullable.GetUnderlyingType(x.PropertyType) ?? x.PropertyType)).ToArray());

            foreach (var item in collection) {
                var row = table.NewRow();

                foreach (var prop in props) {
                    var value = prop.GetValue(item, null);

                    row[prop.Name] = value ?? DBNull.Value;
                }

                table.Rows.Add(row);
            }





            return table;
        }


        /// <summary>
        /// ������� <see cref="DataTable"/>, ���������� ���� ������, ����������������� �� �������. � �������� ������� ������������ ���������� ��������, ���� ��� ��������� ���� �� ���� �� ��������.
        /// ������ ���������: ��� ��������� <see cref="DataTable"/> ������������ ���������.
        /// </summary>
        public static DataTable ToSingleDataTable<T>(this T entity, params Expression<Func<T, object>>[] expressions) {
            var type = typeof (T);
            PropertyInfo[] props;

            if (expressions.Any()) {
                var includedNames = expressions.MemberExpressions().Select(x => x.Member.Name);
                props = entity.GetType().GetProperties().Where(x => x.CanRead && includedNames.Contains(x.Name)).ToArray();
            }
            else {
                props = entity.GetType().GetProperties()
                    .Where(
                        x => x.CanRead &&
                             (x.PropertyType.IsAssignableFrom(_stringType) || (!typeof (IEnumerable).IsAssignableFrom(x.PropertyType) && !x.PropertyType.IsClass)))
                    .ToArray();
            }


            var tableName = type.Name;
            var lastChar = tableName.Last();
            tableName += char.ToLowerInvariant(lastChar).Equals('s') ? "es" : "s";

            var table = new DataTable(tableName);
            table.Columns.AddRange(props.OrderBy(x => x.Name).Select(x => new DataColumn(x.Name, Nullable.GetUnderlyingType(x.PropertyType) ?? x.PropertyType)).ToArray());

            var row = table.NewRow();

            foreach (var prop in props) {
                var value = prop.GetValue(entity, null);

                row[prop.Name] = value ?? DBNull.Value;
            }

            table.Rows.Add(row);


            return table;
        }


        static readonly Type _stringType = typeof (string);



        /// <summary>
        /// ������������ <see cref="DataTable"/> � ��������� ����������� ����.
        /// �����: ��� ����������� ������������ ���������, ��� ������������ � ��������� ���������-�������.
        /// </summary>
        public static List<T> To<T>(this DataTable table) where T : class, new() {
            return table.Rows.Cast<DataRow>().Select(x => x.To<T>()).ToList();
        }
    }
}

#endif
