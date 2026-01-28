using System;
using System.Linq;
using App.Core.Domain.Customers;
using App.Core.Domain.Holidays;
using App.Core.Domain.Security;
using App.Data.Extensions;
using App.Data.Mapping;
using FluentMigrator;

namespace App.Data.Migrations.UpgradeTo460
{
	[NopMigration("2024-02-17 01:02:12", "HolidayMigration for 4.60.0", MigrationProcessType.Update)]
	public class HolidayMigration : Migration
	{
		#region Fields

		private readonly INopDataProvider _dataProvider;

		#endregion

		#region Ctor

		public HolidayMigration(INopDataProvider dataProvider)
		{
			_dataProvider = dataProvider;
		}

		#endregion

		/// <summary>
		/// Collect the UP migration expressions
		/// </summary>
		public override void Up()
		{
			if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(Holiday))).Exists())
			{
				Create.TableFor<Holiday>();
			}

			//#5607
			if (!_dataProvider.GetTable<PermissionRecord>().Any(pr => string.Compare(pr.SystemName, "ManageHoliday", StringComparison.InvariantCultureIgnoreCase) == 0))
			{
				var multifactorAuthenticationPermissionRecord = _dataProvider.InsertEntity(
					new PermissionRecord
					{
						SystemName = "ManageHoliday",
						Name = "Admin area.Manage Holiday",
						Category = "Standard"
					}
				);

				var customerRoles = _dataProvider.GetTable<CustomerRole>().Where(cr => cr.SystemName == NopCustomerDefaults.AdministratorsRoleName);

				foreach (var role in customerRoles.ToList())
				{
					_dataProvider.InsertEntity(
						new PermissionRecordCustomerRoleMapping
						{
							CustomerRoleId = role.Id,
							PermissionRecordId = multifactorAuthenticationPermissionRecord.Id
						}
					);
				}
			}

		}

		public override void Down()
		{
			//add the downgrade logic if necessary 
		}
	}
}
