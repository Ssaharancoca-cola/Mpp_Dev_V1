﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Model.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Model.Models
{
    public partial interface IMPP_ContextProcedures
    {
        Task<int> MPP_ENTITY_SEC_BASE_VIEWS_FN_PROCAsync(int? I_ENTITY_TYPE_ID, string I_USER_ID, OutputParameter<string> I_RESULT, OutputParameter<int> returnValue = null, CancellationToken cancellationToken = default);
        Task<List<MPP_LOAD_CHKResult>> MPP_LOAD_CHKAsync(string i_Session_id, int? i_entity_type_id, string i_user_id, string i_suppress_warnings, OutputParameter<int> returnValue = null, CancellationToken cancellationToken = default);
    }
}
