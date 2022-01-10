using CosmosDbExplorer.ViewModel.Indexes;
using FluentValidation.TestHelper;
using System;
using Xunit;

namespace CosmosDbExplorer.Tests
{
    public class IndexValidationTests
    {
        [Fact]
        public void SimplePathIsValid()
        {
            const string value = @"/headquarters/employees/?";

            var validator = new ExcludedPathViewModelValidator();
            validator.ShouldNotHaveValidationErrorFor(idx => idx.Path, value);
        }

        [Fact]
        public void ArrayPathIsValid()
        {
            const string value = @"/locations/[]/country/?";

            var validator = new ExcludedPathViewModelValidator();
            validator.ShouldNotHaveValidationErrorFor(idx => idx.Path, value);
        }

        [Fact]
        public void PathToAnythingIsValid()
        {
            const string value = @"/headquarters/*";

            var validator = new ExcludedPathViewModelValidator();
            validator.ShouldNotHaveValidationErrorFor(idx => idx.Path, value);
        }

        [Fact]
        public void EtagPathIsValid()
        {
            const string value = @"/""_etag""/?";

            var validator = new ExcludedPathViewModelValidator();
            validator.ShouldNotHaveValidationErrorFor(idx => idx.Path, value);
        }
    }
}
