// <copyright file="FinancialExportService.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Application.Dto.Transaction;
using PayTrack.Application.Services.Model;
using PayTrack.Data.Repositories.Model;

namespace PayTrack.Application.Services.Implementation
{
    /// <inheritdoc/>
    public class FinancialExportService(ITransactionRepository _transactionRepository) : IFinancialExportService
    {
        private readonly ITransactionRepository transactionRepository = _transactionRepository;

        /// <inheritdoc/>
        public async Task<FinancialExportResult> ExportFinancialDataAsync(GetTransactionQuery query)
        {
            _ = await this.transactionRepository.GetAllAsync(query);

            throw new NotImplementedException("Financial export generation is implemented in the next step.");
        }
    }
}
