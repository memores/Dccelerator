using System;
using System.Data;
using System.Data.SqlClient;
using JetBrains.Annotations;


namespace Dccelerator.DataAccess.Ado.SqlClient
{
    public static class SqlClientDataTableExtensions {

        /// <summary>
        /// ��������� ������� � ���� ������. ����� ������ ���������
        /// </summary>
        /// <exception cref="ArgumentException">���������� �������� null ��� ������ ������ (""), � ������� ����������� ���������. </exception>
        /// <exception cref="DuplicateNameException">������� ����������� ���������, ������� ��� �������� ������� � ����� �� ������. (��� ��������� ����������� �������).</exception>
        /// <seealso cref="DataTableUtils.ToDataTable{T}"/>
        public static void BulkInsert([NotNull] this DataTable table, [NotNull] string connection) {
            using (var bulk = new SqlBulkCopy(connection) {
                DestinationTableName = table.TableName,
                BatchSize = 5000,
                BulkCopyTimeout = 1200
            }) {
                bulk.WriteToServer(table);
            }
        }
    }
}