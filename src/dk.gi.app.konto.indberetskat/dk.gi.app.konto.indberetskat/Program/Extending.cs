using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dk.gi.app.konto.indberetskat
{
	internal static class Extending
	{
		internal static List<T[]> ToBatchList<T>(this IEnumerable<T> samling, int batchStoerrelse)
		{
			List<T[]> resultat = new List<T[]>();
			List<T> aktuelBatch = new List<T>();

			foreach (T item in samling)
			{
				if(aktuelBatch.Count == batchStoerrelse)
				{
					resultat.Add(aktuelBatch.ToArray());
					aktuelBatch.Clear();
				}

				aktuelBatch.Add(item);
			}

			if(aktuelBatch.Any())
				resultat.Add(aktuelBatch.ToArray());

			return resultat;
		}
	}
}
