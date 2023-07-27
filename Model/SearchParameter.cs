using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class SearchParameter
    {
        public enum SearchCompareType
        {
            Like,
            Equal,
            IN,
            Custom,
            GreaterthanOrEqual,
            LessthanOrEqual,
            Between
        }
        public SearchParameter()
        {
            CompareType = SearchCompareType.Like;
        }
        public string DBFieldName { get; set; }
        public string SearchValue { get; set; }
        public SearchCompareType CompareType { get; set; }
        public string DataType { get; set; }
        
    }
}
