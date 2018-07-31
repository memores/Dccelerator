#if !NETSTANDARD1_3

using System;
using System.Collections.Generic;
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
        /// <seealso cref="DataTableUtils.ToDataTable{T}(IEnumerable{T}, string[], string)"/>
        public static void BulkInsert([NotNull] this DataTable table, [NotNull] DbActionArgs args, SqlBulkCopyOptions options = SqlBulkCopyOptions.Default, int batchSize = 5000, int timeout = 1200) {
            using (var bulk = new SqlBulkCopy((SqlConnection)args.Connection, options, (SqlTransaction)args.Transaction) {
                DestinationTableName = table.TableName,
                BatchSize = batchSize,
                BulkCopyTimeout = timeout
            }) {
                bulk.WriteToServer(table);
            }
        }


        /// <summary>
        /// ��������� ������� � ���� ������. ����� ������ ���������
        /// </summary>
        /// <exception cref="ArgumentException">���������� �������� null ��� ������ ������ (""), � ������� ����������� ���������. </exception>
        /// <exception cref="DuplicateNameException">������� ����������� ���������, ������� ��� �������� ������� � ����� �� ������. (��� ��������� ����������� �������).</exception>
        /// <seealso cref="DataTableUtils.ToDataTable{T}(IEnumerable{T}, string[], string)"/>
        public static void BulkInsert([NotNull] this DataTable table, [NotNull] string connection, SqlBulkCopyOptions options = SqlBulkCopyOptions.Default, int batchSize = 5000, int timeout = 1200) {
            using (var bulk = new SqlBulkCopy(connection, options) {
                DestinationTableName = table.TableName,
                BatchSize = batchSize,
                BulkCopyTimeout = timeout
            }) {
                bulk.WriteToServer(table);
            }
        }


        /// <summary>
        /// ��������� ������� � ���� ������. ����� ������ ���������
        /// </summary>
        /// <exception cref="ArgumentException">���������� �������� null ��� ������ ������ (""), � ������� ����������� ���������. </exception>
        /// <exception cref="DuplicateNameException">������� ����������� ���������, ������� ��� �������� ������� � ����� �� ������. (��� ��������� ����������� �������).</exception>
        /// <seealso cref="DataTableUtils.ToDataTable{T}(IEnumerable{T}, string[], string)"/>
        public static void BulkInsert([NotNull] this DataTable table, [NotNull] SqlConnection connection, [NotNull] SqlTransaction transaction, SqlBulkCopyOptions options = SqlBulkCopyOptions.Default, int batchSize = 5000, int timeout = 1200) {
            using (var bulk = new SqlBulkCopy(connection, options, transaction) {
                DestinationTableName = table.TableName,
                BatchSize = batchSize,
                BulkCopyTimeout = timeout
            }) {
                bulk.WriteToServer(table);
            }
        }

    }
}

#endif
