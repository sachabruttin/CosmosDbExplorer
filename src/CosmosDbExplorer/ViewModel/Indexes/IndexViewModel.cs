using System;
using System.Collections.Generic;
using System.ComponentModel;
using FluentValidation;
using GalaSoft.MvvmLight;
using Microsoft.Azure.Documents;
using PropertyChanged;
using Validar;

namespace CosmosDbExplorer.ViewModel.Indexes
{
    [InjectValidation]
    public class IndexViewModel : ObservableObject, IEquatable<IndexViewModel>
    {
        public IndexViewModel()
        {
        }

        public IndexViewModel(Index index)
        {
            switch (index)
            {
                case RangeIndex rangeIndex:
                    DataType = rangeIndex.DataType;
                    Precision = rangeIndex.Precision;
                    IsMaxPrecision = rangeIndex.Precision == -1;
                    Kind = rangeIndex.Kind;
                    break;
                case HashIndex hashIndex:
                    DataType = hashIndex.DataType;
                    Precision = hashIndex.Precision;
                    Kind = hashIndex.Kind;
                    break;
                case SpatialIndex spatialIndex:
                    DataType = spatialIndex.DataType;
                    Precision = null;
                    Kind = spatialIndex.Kind;
                    break;
                default:
                    throw new Exception("Index Type unknown");
            }
        }

        public bool HasErrors => ((INotifyDataErrorInfo)this).HasErrors;

        public Index GetIndex()
        {
            switch (Kind)
            {
                case IndexKind.Hash:
                    return new HashIndex(DataType.Value, Precision.Value);
                case IndexKind.Range:
                    return new RangeIndex(DataType.Value, Precision.Value);
                default:
                    return new SpatialIndex(DataType.Value);
            }
        }

        public IndexKind? Kind { get; set; }

        protected void OnKindChanged()
        {
            switch (Kind)
            {
                case IndexKind.Hash:
                    MinPrecision = 1;
                    MaxPrecision = 8;

                    if (Precision.GetValueOrDefault(3) > MaxPrecision)
                    {
                        Precision = 3;
                    }
                    break;
                case IndexKind.Range:
                    MinPrecision = 1;
                    MaxPrecision = 100;
                    break;
                case IndexKind.Spatial:
                    Precision = null;
                    break;
            }
        }

        public DataType? DataType { get; set; }

        [DependsOn(nameof(Kind))]
        public DataType[] AvailableDataTypes
        {
            get
            {
                switch (Kind)
                {
                    case IndexKind.Spatial:
                        return new [] { Microsoft.Azure.Documents.DataType.Point, Microsoft.Azure.Documents.DataType.Polygon, Microsoft.Azure.Documents.DataType.LineString };
                    default:
                        return new [] { Microsoft.Azure.Documents.DataType.String, Microsoft.Azure.Documents.DataType.Number };
                }
            }
        }

        public short? Precision { get; set; }

        public short MinPrecision { get; set; }

        public short MaxPrecision { get; set; }

        public bool IsMaxPrecision { get; set; }

        protected void OnIsMaxPrecisionChanged()
        {
            if (IsMaxPrecision)
            {
                Precision = -1;
            }
            else
            {
                Precision = 3;
            }
        }

        public bool Equals(IndexViewModel other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return DataType == other.DataType;
        }

        public override int GetHashCode()
        {
            return DataType.GetValueOrDefault().GetHashCode();
        }

        [DependsOn(nameof(IsMaxPrecision), nameof(Kind))]
        public bool CanSetPrecision => !IsMaxPrecision && Kind != IndexKind.Spatial;

        [DependsOn(nameof(Kind))]
        public bool CanSetMaxPrecision => Kind == IndexKind.Range;
    }

    public class IndexViewModelValidator : AbstractValidator<IndexViewModel>
    {
        public IndexViewModelValidator()
        {
            RuleFor(x => x.Kind).NotEmpty();
            RuleFor(x => x.DataType).NotEmpty();
            RuleFor(x => x.Precision).NotEmpty().When(x => x.Kind.HasValue && x.Kind.Value != IndexKind.Spatial);
        }
    }
}
