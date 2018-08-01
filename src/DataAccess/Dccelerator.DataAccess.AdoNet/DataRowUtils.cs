#if !NETSTANDARD1_3

using System;
using System.Data;
using System.Diagnostics;
using Dccelerator.UnFastReflection;
using Dccelerator.UnSmartConvertion;
using JetBrains.Annotations;


namespace Dccelerator.DataAccess.Ado {
    public static class DataRowUtils {
        /// <summary>
        /// ������������ <see cref="DataRow"/> � ����������� ����.
        /// �����: ��� ����������� ������������ ���������, ��� ������������ � ��������� ���������-�������.
        /// </summary>
        [NotNull]
        public static T To<T>([NotNull] this DataRow row) where T : class, new() {
            var item = new T();
            return row.To(item);
        }


        /// <summary>
        /// ������������ <see cref="DataRow"/> � ����������� ����.
        /// �����: ��� ����������� ������������ ���������, ��� ������������ � ��������� ���������-�������.
        /// </summary>
        [NotNull]
        public static object To([NotNull] this DataRow row, [NotNull] Type type) {
            var item = Activator.CreateInstance(type);
            return row.To(item);
        }


        /// <summary>
        /// ������������ <see cref="DataRow"/> � ����������� ����.
        /// �����: ��� ����������� ������������ ���������, ��� ������������ � ��������� ���������-�������.
        /// </summary>
        /// <exception cref="DeletedRowInaccessibleException">���� ����������� ������� ������ �������� � ��������� ������. </exception>
        /// <exception cref="InvalidCastException">���� ������ �������� � ������� �� ���������. </exception>
        /// <exception cref="ArgumentException">������� �� ����������� ���� �������. </exception>
        /// <exception cref="ArgumentNullException">��������� ��������� <paramref name="column" /> �������� null. </exception>
        /// <exception cref="DuplicateNameException">������� � ��� �� ������ ��� ���������� � ���������.��������� ����� ����������� ��� ����� ��������.</exception>
        [NotNull]
        public static T To<T>([NotNull] this DataRow row, [NotNull] T item) where T : class {
            foreach (DataColumn column in row.Table.Columns) {
                var value = row[column];
                if (value == DBNull.Value || value == null)
                    continue;

                if (!RUtils<T>.TrySet(item, column.ColumnName, value))
                    Log.TraceEvent(TraceEventType.Warning, $"Can't set property {column.ColumnName} from '{typeof (T).FullName}' context.");
            }

            return item;
        }


        /// <summary>
        /// ������������ <see cref="DataRow"/> � ����������� ����.
        /// �����: ��� ����������� ������������ ���������, ��� ������������ � ��������� ���������-�������.
        /// </summary>
        [NotNull]
        public static object To([NotNull] this DataRow row, [NotNull] object item) {
            foreach (DataColumn column in row.Table.Columns) {
                var value = row[column];
                if (value == DBNull.Value || value == null)
                    continue;

                if (!item.TrySet(column.ColumnName, value))
                    Log.TraceEvent(TraceEventType.Warning, $"Can't set property {column.ColumnName} from '{item.GetType().FullName}' context.");
            }

            return item;
        }


        /// <summary>
        /// ���������� null, ���� <paramref name="column"/> � ���������� ������ == <see cref="DBNull"/>, ��� �� ������ - � �������� ������
        /// </summary>

        public static string ToStringOrNull(this DataRow row, string column) {
            if (row == null)
                return null;

            var cell = row.Table.Columns.Contains(column) ? row[column] : DBNull.Value;
            return cell != DBNull.Value ? cell.ToStringOrNull() : null;
        }


        /// <summary>
        /// ���������� string.empty, ���� <paramref name="column"/> � ���������� ������ == <see cref="DBNull"/>, ��� �� ������ - � �������� ������
        /// </summary>

        public static string ToStringOrEmpty(this DataRow row, string column) {
            if (row == null)
                return string.Empty;

            var cell = row.Table.Columns.Contains(column) ? row[column] : DBNull.Value;
            return cell != DBNull.Value ? cell.ToStringOrEmpty() : string.Empty;
        }


        /// <summary>
        /// ���������� null, ���� <paramref name="column"/> � ���������� ������ == <see cref="DBNull"/>, ��� ���������� �������� �������� � Guid.
        /// </summary>

        public static Guid? ToGuidOrNull(this DataRow row, string column) {
            var cell = row.Table.Columns.Contains(column) ? row[column] : DBNull.Value;
            if (cell == DBNull.Value)
                return null;

            if (cell is Guid)
                return (Guid) cell;

            Guid guid;
            return Guid.TryParse(cell.ToString(), out guid) ? (Guid?) guid : null;
        }


        /// <summary>
        /// ���������� Guid.Empty, ���� <paramref name="column"/> � ���������� ������ == <see cref="DBNull"/>, ��� ���������� �������� �������� � Guid.
        /// </summary>
        public static Guid ToGuidOrEmpty(this DataRow row, string column) {
            var cell = row.Table.Columns.Contains(column) ? row[column] : DBNull.Value;
            if (cell == DBNull.Value)
                return Guid.Empty;

            if (cell is Guid)
                return (Guid) cell;

            Guid guid;
            return Guid.TryParse(cell.ToString(), out guid) ? guid : Guid.Empty;
        }


        /// <summary>
        /// ���������� null, ���� <paramref name="column"/> � ���������� ������ == <see cref="DBNull"/>, ��� ���������� �������� �������� � DateTime.
        /// </summary>
        public static DateTime? ToDateTimeOrNull(this DataRow row, string column) {
            var cell = row.Table.Columns.Contains(column) ? row[column] : DBNull.Value;
            if (cell == DBNull.Value)
                return null;

            if (cell is DateTime)
                return (DateTime?) cell;

            DateTime time;
            return DateTime.TryParse(cell.ToString(), out time) ? (DateTime?) time : null;
        }



        /// <summary>
        /// Returns <see landword="null"/>, if <paramref name="column"/> == <see cref="DBNull"/>, or if value can't be parsed info <see landword="decimal"/>.
        /// </summary>
        public static decimal? ToDecimalOrNull(this DataRow row, string column) {
            var cell = row.Table.Columns.Contains(column) ? row[column] : DBNull.Value;
            if (cell == DBNull.Value)
                return null;

            if (cell is decimal)
                return (decimal?) cell;

            decimal value;
            return decimal.TryParse(cell.ToString(), out value) ? (decimal?) value : null;
        }



        /// <summary>
        /// Returns <see cref="decimal.Zero"/>, if <paramref name="column"/> == <see cref="DBNull"/>, or if value can't be parsed info <see landword="decimal"/>.
        /// </summary>
        public static decimal ToDecimalOrZero(this DataRow row, string column) {
            var cell = row.Table.Columns.Contains(column) ? row[column] : DBNull.Value;
            if (cell == DBNull.Value)
                return decimal.Zero;

            if (cell is decimal)
                return (decimal) cell;

            decimal value;
            return decimal.TryParse(cell.ToString(), out value) ? value : decimal.Zero;
        }






        /// <summary>
        /// ���������� <paramref name="defaultTime"/>, ���� <paramref name="column"/> � ���������� ������ == <see cref="DBNull"/>, ��� ���������� �������� �������� � DateTime.
        /// </summary>
        public static DateTime ToDateTimeOrDefault(this DataRow row, string column, DateTime defaultTime = default(DateTime)) {
            var cell = row.Table.Columns.Contains(column) ? row[column] : DBNull.Value;
            if (cell == DBNull.Value)
                return defaultTime;

            if (cell is DateTime)
                return (DateTime) cell;

            DateTime time;
            return DateTime.TryParse(cell.ToString(), out time) ? time : defaultTime;
        }




        /// <summary>
        /// ���������� 0, ���� <paramref name="column"/> � ���������� ������ == <see cref="DBNull"/>, ��� ���������� �������� �������� � int.
        /// </summary>
        public static int ToIntOrZero(this DataRow row, string column) {
            var cell = row.Table.Columns.Contains(column) ? row[column] : DBNull.Value;
            if (cell == DBNull.Value)
                return 0;

            if (cell is int)
                return (int) cell;

            int value;
            return int.TryParse(cell.ToString(), out value) ? value : 0;
        }


        /// <summary>
        /// ���������� null, ���� <paramref name="column"/> � ���������� ������ == <see cref="DBNull"/>, ��� ���������� �������� �������� � int.
        /// </summary>
        public static int? ToIntOrNull(this DataRow row, string column) {
            var cell = row.Table.Columns.Contains(column) ? row[column] : DBNull.Value;
            if (cell == DBNull.Value)
                return null;

            if (cell is int)
                return (int) cell;

            int value;
            return int.TryParse(cell.ToString(), out value) ? (int?) value : null;
        }


        public static bool ToBool(this DataRow row, string column, bool defaultValue) {
            var cell = row.Table.Columns.Contains(column) ? row[column] : DBNull.Value;
            if (cell == DBNull.Value)
                return defaultValue;

            if (cell is bool)
                return (bool) cell;

            if (cell is int)
                return row.ToIntOrZero(column) != 0;

            return false;
        }
    }
}


#endif
