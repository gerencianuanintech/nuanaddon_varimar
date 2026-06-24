using nuanaddon_varimar.DTO;
using nuanaddon_varimar.Infrastructure.Sap;
using nuanaddon_varimar.Shared.Constants;
using nuanaddon_varimar.Shared.Messages;
using System;
using System.Collections.Generic;

namespace nuanaddon_varimar.Application.Services {
    public class LotConfigurationService : ILotConfigurationService {
        private const int DefaultBufferDays = 7;
        private const string BufferDaysParameter = "DiasBufferLotes";
        private const string SelectionCriterionParameter = "CriterioSeleccionLotes";

        public LotSelectionConfigDto GetConfiguration() {
            Dictionary<string, string> parameters = ReadParameters();

            return new LotSelectionConfigDto {
                BufferDays = GetBufferDays(parameters),
                SelectionCriterion = GetSelectionCriterion(parameters)
            };
        }

        private Dictionary<string, string> ReadParameters() {
            Dictionary<string, string> parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string query =
                "SELECT \"Code\", \"Name\" " +
                "FROM \"@IZ_PARAM\" " +
                "WHERE \"Code\" IN ('DiasBufferLotes', 'CriterioSeleccionLotes')";

            SapRecordsetExecutor.Execute(query, "Application.Services.LotConfigurationService.cs -> ReadParameters", recordset => {
                while (!recordset.EoF) {
                    string code = Convert.ToString(recordset.Fields.Item("Code").Value);
                    string value = Convert.ToString(recordset.Fields.Item("Name").Value);

                    if (!string.IsNullOrWhiteSpace(code))
                        parameters[code.Trim()] = value;

                    recordset.MoveNext();
                }
            });

            return parameters;
        }

        private int GetBufferDays(Dictionary<string, string> parameters) {
            string rawValue;
            int bufferDays;

            if (!parameters.TryGetValue(BufferDaysParameter, out rawValue) ||
                string.IsNullOrWhiteSpace(rawValue) ||
                !int.TryParse(rawValue.Trim(), out bufferDays) ||
                bufferDays < 0) {
                WarnIfSapUiAvailable(LotAssignmentMessages.InvalidBufferDaysParameter);
                return DefaultBufferDays;
            }

            return bufferDays;
        }

        private string GetSelectionCriterion(Dictionary<string, string> parameters) {
            string rawValue;

            if (!parameters.TryGetValue(SelectionCriterionParameter, out rawValue) ||
                string.IsNullOrWhiteSpace(rawValue)) {
                WarnIfSapUiAvailable(LotAssignmentMessages.InvalidSelectionCriterionParameter);
                return LotSelectionCriteria.MenorCantidad;
            }

            string criterion = rawValue.Trim().ToUpperInvariant();
            if (!LotSelectionCriteria.ValidCriteria.Contains(criterion)) {
                WarnIfSapUiAvailable(LotAssignmentMessages.InvalidSelectionCriterionParameter);
                return LotSelectionCriteria.MenorCantidad;
            }

            return criterion;
        }

        private void WarnIfSapUiAvailable(string message) {
            if (Program.SBOApplication != null)
                SapMessages.Warning(message);
        }
    }
}
