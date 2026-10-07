using Activadis.Domain.Entities;
using Activadis.Domain.Filters;
using System;
using System.Collections.Generic;
using System.Text;

namespace Activadis.Infrastructure.Extensions
{
	public static class ActivityQueryExtensions
	{
		public static IQueryable<Activity> ApplyFilter(this IQueryable<Activity> activities, bool isAdmin, ActivityFilter filter)
			=> activities
				.Where(x => x.DeletedAt == null)
				.Where(x => isAdmin || x.PublishedOn != null)
				.Where(x => (isAdmin && filter.IncludePast) || x.EndDate >= DateTime.UtcNow)
				.Where(x => filter.Name == null || x.Name.Contains(filter.Name))
				.Where(x => filter.To == null || x.StartDate <= filter.To)
				.Where(x => filter.From == null || x.EndDate >= filter.From);
	}
}
