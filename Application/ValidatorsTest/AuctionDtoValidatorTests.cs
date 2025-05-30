using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation.TestHelper;
using MSAuction.Application.DTOs;
using MSAuction.Application.Validators;

namespace MsAuctionsTests.Application.ValidatorsTest
{
    public class AuctionDtoValidatorTests
    {
        private readonly AuctionDtoValidator _validator;

        public AuctionDtoValidatorTests()
        {
            _validator = new AuctionDtoValidator();
        }

        [Fact]
        public void Should_Have_Error_When_Title_Is_Empty()
        {
            var dto = new AuctionDto { Title = "" };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Title);
        }

        [Fact]
        public void Should_Have_Error_When_Title_Exceeds_MaxLength()
        {
            var dto = new AuctionDto { Title = new string('A', 101) };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Title);
        }

        [Fact]
        public void Should_Have_Error_When_InitialPrice_Is_Less_Than_Or_Equal_To_Zero()
        {
            var dto = new AuctionDto { InitialPrice = 0 };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.InitialPrice);
        }

        [Fact]
        public void Should_Have_Error_When_MinIncrement_Is_Less_Than_Or_Equal_To_Zero()
        {
            var dto = new AuctionDto { MinIncrement = 0 };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.MinIncrement);
        }

        [Fact]
        public void Should_Have_Error_When_StartTime_Is_Not_Less_Than_EndTime()
        {
            var dto = new AuctionDto
            {
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddMinutes(-1)
            };

            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.StartTime);
        }

        [Fact]
        public void Should_Not_Have_Errors_When_Valid()
        {
            var dto = new AuctionDto
            {
                Title = "Valid Title",
                InitialPrice = 10,
                MinIncrement = 1,
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(1)
            };

            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
