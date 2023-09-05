using System.Text;
using System.Linq.Dynamic.Core;
using CourseLibrary.API.Services.PropertyMappingService;

namespace CourseLibrary.API.Helpers;

public static class IQueryableExtensions
{
    public static IQueryable<T> ApplySort<T>(
        this IQueryable<T> source,
        string orderBy,
        Dictionary<string, PropertyMappingValue> mappingDictionary)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        if (mappingDictionary == null)
            throw new ArgumentNullException(nameof(mappingDictionary));

        if (string.IsNullOrWhiteSpace(orderBy))
            return source;

        StringBuilder orderByString = new StringBuilder();

        // the orderBy string is separated by "," so we need to split it
        var orderByAfterSplit = orderBy.Split(',');

        // apply each orderBy clause
        foreach (var orderByClause in orderByAfterSplit)
        {
            // trim the orderByClause, as it may contain leading or trailing spaces
            // can't trim the var in foreach, so we use another var. 
            var trimmedOrderByClause = orderByClause.Trim();

            // if the sort option ends with "desc", we order descending, otherwise ascending
            var orderDescending = trimmedOrderByClause.EndsWith(" desc");

            // remove "asc" or "desc" from the orferBy clause, to get the property
            // name to look for in the mapping dictionary
            var indexOfFirstSpace = trimmedOrderByClause.IndexOf(' ');
            var propertyName = indexOfFirstSpace == -1
                ? trimmedOrderByClause
                : trimmedOrderByClause.Remove(indexOfFirstSpace);

            // find matching property
            if (!mappingDictionary.ContainsKey(propertyName))
                throw new ArgumentException($"Key mapping for {propertyName} is missing");

            // get the PropertyMappingValue
            var propertyMappingValue = mappingDictionary[propertyName];
            if (propertyMappingValue == null)
                throw new ArgumentNullException("propertyMappingValue");

            // revert sort order if necessary
            if (propertyMappingValue.Revert)
                orderDescending = !orderDescending;

            // run through property names
            foreach (var desinationProperty in propertyMappingValue.DestinationPropeties)
            {
                orderByString.Append(
                      (string.IsNullOrWhiteSpace(orderByString.ToString()) ? string.Empty : ",")
                    + desinationProperty
                    + (orderDescending ? " descending" : " ascending"));
            }
        }

        return source.OrderBy(orderByString.ToString());
    }
}
