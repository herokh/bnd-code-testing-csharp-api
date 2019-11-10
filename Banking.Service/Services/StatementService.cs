﻿using Banking.Application.DTOs;
using Banking.Application.Helpers;
using Banking.Application.Models;
using Banking.Application.Validations;
using Banking.Infrastructure.Repositories.EFCore;
using Banking.Service.Services.Interfaces;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Banking.Service.Services
{
    public class StatementService : IStatementService
    {
        private readonly StatementDepositValidator _validator;

        private readonly AccountRepository _accountRepository;
        private readonly StatementRepository _statementRepository;

        private readonly AppSettings _appSettings;
        public StatementService(
            StatementDepositValidator validator,
            AccountRepository accountRepository,
            StatementRepository statementRepository,
            IOptions<AppSettings> appSettings)
        {
            _validator = validator;
            _accountRepository = accountRepository;
            _statementRepository = statementRepository;

            _appSettings = appSettings.Value;
        }

        public async Task<StatementDto> Deposit(StatementDepositDto dto)
        {
            ValidationHelper.Validate(_validator, dto);
            var account = await _accountRepository.GetByIBanNumber(dto.iban_number);

            var statement = new Statement
            {
                Account = account,
                Amount = dto.amount,
                CreateAt = DateTime.Now,
                Fee = _appSettings.Fee,
                StatementType = dto.statement_type
            };
            await _statementRepository.Add(statement);

            return ConvertToDto(statement);
        }

        public async Task<StatementDto> Get(int id)
        {
            var entity = await _statementRepository.Get(id);

            return ConvertToDto(entity);
        }

        public async Task<IEnumerable<StatementDto>> GetAll(string ibanNumber)
        {
            var account = await _accountRepository.GetByIBanNumber(ibanNumber);
            var statements = await _statementRepository.GetByAccountId(account.Id);

            return statements.Select(x => ConvertToDto(x));
        }

        private StatementDto ConvertToDto(Statement entity)
        {
            return new StatementDto
            {
                id = entity.Id,
                amount = entity.Amount,
                fee_as_percent = entity.Fee,
                create_at = entity.CreateAt,
                statement_type = entity.StatementType
            };
        }

    }
}