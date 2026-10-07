using System;
using System.Collections.Generic;
using System.Text;

namespace Activadis.Domain.Filters
{
	public class ActivityFilter
	{
		public string? Name { get; set; }
		public DateTime? From { get; set; }
		public DateTime? To { get; set; }
		public bool IncludePast { get; set; }
	}
}
