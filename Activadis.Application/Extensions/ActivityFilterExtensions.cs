using Activadis.Domain.Filters;
using Activadis.Shared.DTOs.Activity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Activadis.Application.Extensions
{
	public static class ActivityFilterExtensions
	{
		public static ActivityFilter ToFilter(this ActivityFilterRequest request, bool isAdmin)
		{
			return new ActivityFilter()
			{
				Name = string.IsNullOrWhiteSpace(request.Name) ? null : request.Name.Trim(),
				From = request.From,
				To = request.To,
				IncludePast = isAdmin && request.IncludePast
			};
		}
	}
}
