using nuanaddon_varimar.DTO;
using nuanaddon_varimar.Infrastructure.Sap;
using nuanaddon_varimar.Shared.Constants;
using nuanaddon_varimar.Shared.Messages;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace nuanaddon_varimar.Application.Services {
    public class LotConfigurationService : ILotConfigurationService {
        private const int DefaultBufferDays = 7;
        private const string BufferDaysCode = "DiasBufferLotes";
        private const string SelectionCriterionCode = "CriterioSeleccionLotes";

        public LotSelectionConfigDto GetConfig() {
            LotSelectionConfigDto config = new LotSelectionConfigDto();
            IDictionary<string, string> parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            string query =
                "SELECT \"Code\", \"Name\" " +
                "FROM \"@IZ_PARAM\" " +
                "WHERE \"Code\" IN ('DiasBufferLotes', 'CriterioSeleccionLotes')";

            SapRecordsetExecutor.Execute(query, "Application.Services.LotConfigurationService.cs -> GetConfig", recordset => {
                while (!recordset.EoF) {
                    string code = Convert.ToString(recordset.Fields.Item("Code").Value);
                    string value = Convert.ToString(recordset.Fields.Item("Name").Value);

                    if (!string.IsNullOrWhiteSpace(code))
                        parameters[code.Trim()] = value;

                    recordset.MoveNext();
                }
            });

            config.BufferDays = ResolveBufferDays(parameters);
            config.SelectionCriterion = ResolveSelectionCriterion(parameters);
            return config;
        }

        private int ResolveBufferDays(IDictionary<string, string> parameters) {
            string value;
            if (!parameters.TryGetValue(BufferDaysCode, out value))
                return DefaultBufferDays;

            int bufferDays;
            if (!int.TryParse((value ?? string.Empty).Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out bufferDays) || bufferDays < 0) {
                SapMessages.Warning(LotAssignmentMessages.InvalidBufferDaysParameter);
                return DefaultBufferDays;
            }

            return bufferDays;
        }

        private string ResolveSelectionCriterion(IDictionary<string, string> parameters) {
            string value;
            if (!parameters.TryGetValue(SelectionCriterionCode, out value))
                return LotSelectionCriteria.MenorCantidad;

            if (!LotSelectionCriteria.IsValid(value)) {
                SapMessages.Warning(LotAssignmentMessages.InvalidSelectionCriterionParameter);
                return LotSelectionCriteria.MenorCantidad;
            }

            return LotSelectionCriteria.NormalizeOrDefault(value);
        }
    }
}
