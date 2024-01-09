using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payment.Applications.Base.Models
{
    // Class này nhận vào 1 kiểu dữ liệu bất kỳ, nên để là kiểu "T"
    public class BaseResultWithData<T> : BaseResult
    {
        public T? Data { get; set; }
    }
}
