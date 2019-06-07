using System;
using System.Linq;
using Unity.Properties;

namespace Unity.Serialization
{
    /// <summary>
    /// Provides the properties API over the generic <see cref="SerializedObjectView"/>.
    /// </summary>
    public class SerializedObjectViewPropertyBag : PropertyBag<SerializedObjectView>
    {
        private struct SerializedObjectViewProperty : IProperty<SerializedObjectView, SerializedObjectView>
        {
            private readonly SerializedStringView m_Name;
            private readonly SerializedValueView m_Value;
            private readonly IPropertyAttributeCollection m_Attributes;

            public SerializedObjectViewProperty(SerializedStringView name, SerializedValueView value, IPropertyAttributeCollection attributes = null)
            {
                m_Name = name;
                m_Value = value;
                m_Attributes = attributes;
            }

            public string GetName() => m_Name.ToString();
            public bool IsReadOnly => true;
            public bool IsContainer => false;
            public IPropertyAttributeCollection Attributes => m_Attributes;
            public SerializedObjectView GetValue(ref SerializedObjectView container) => m_Value.AsObjectView();
            public void SetValue(ref SerializedObjectView container, SerializedObjectView value) => throw new NotSupportedException();
        }

        private struct StringViewProperty : IProperty<SerializedObjectView, SerializedStringView>
        {
            private readonly SerializedStringView m_Name;
            private readonly SerializedValueView m_Value;
            private readonly IPropertyAttributeCollection m_Attributes;

            public StringViewProperty(SerializedStringView name, SerializedValueView value, IPropertyAttributeCollection attributes = null)
            {
                m_Name = name;
                m_Value = value;
                m_Attributes = attributes;
            }

            public string GetName() => m_Name.ToString();
            public bool IsReadOnly => true;
            public bool IsContainer => false;
            public IPropertyAttributeCollection Attributes => m_Attributes;
            public SerializedStringView GetValue(ref SerializedObjectView container) => m_Value.AsStringView();
            public void SetValue(ref SerializedObjectView container, SerializedStringView value) => throw new InvalidOperationException("Property is ReadOnly");
        }

        private struct Int64Property : IProperty<SerializedObjectView, long>
        {
            private readonly SerializedStringView m_Name;
            private readonly SerializedValueView m_Value;
            private readonly IPropertyAttributeCollection m_Attributes;

            public Int64Property(SerializedStringView name, SerializedValueView value, IPropertyAttributeCollection attributes = null)
            {
                m_Name = name;
                m_Value = value;
                m_Attributes = attributes;
            }

            public string GetName() => m_Name.ToString();
            public bool IsReadOnly => true;
            public bool IsContainer => false;
            public IPropertyAttributeCollection Attributes => m_Attributes;
            public long GetValue(ref SerializedObjectView container) => m_Value.AsInt64();
            public void SetValue(ref SerializedObjectView container, long value) => throw new InvalidOperationException("Property is ReadOnly");
        }

        private struct UInt64Property : IProperty<SerializedObjectView, ulong>
        {
            private readonly SerializedStringView m_Name;
            private readonly SerializedValueView m_Value;
            private readonly IPropertyAttributeCollection m_Attributes;

            public UInt64Property(SerializedStringView name, SerializedValueView value, IPropertyAttributeCollection attributes = null)
            {
                m_Name = name;
                m_Value = value;
                m_Attributes = attributes;
            }

            public string GetName() => m_Name.ToString();
            public bool IsReadOnly => true;
            public bool IsContainer => false;
            public IPropertyAttributeCollection Attributes => m_Attributes;
            public ulong GetValue(ref SerializedObjectView container) => m_Value.AsUInt64();
            public void SetValue(ref SerializedObjectView container, ulong value) => throw new InvalidOperationException("Property is ReadOnly");
        }

        private struct FloatProperty : IProperty<SerializedObjectView, float>
        {
            private readonly SerializedStringView m_Name;
            private readonly SerializedValueView m_Value;
            private readonly IPropertyAttributeCollection m_Attributes;

            public FloatProperty(SerializedStringView name, SerializedValueView value, IPropertyAttributeCollection attributes = null)
            {
                m_Name = name;
                m_Value = value;
                m_Attributes = attributes;
            }

            public string GetName() => m_Name.ToString();
            public bool IsReadOnly => true;
            public bool IsContainer => false;
            public IPropertyAttributeCollection Attributes => m_Attributes;
            public float GetValue(ref SerializedObjectView container) => m_Value.AsFloat();
            public void SetValue(ref SerializedObjectView container, float value) => throw new InvalidOperationException("Property is ReadOnly");
        }

        private struct BoolProperty : IProperty<SerializedObjectView, bool>
        {
            private readonly SerializedStringView m_Name;
            private readonly SerializedValueView m_Value;
            private readonly IPropertyAttributeCollection m_Attributes;

            public BoolProperty(SerializedStringView name, SerializedValueView value, IPropertyAttributeCollection attributes = null)
            {
                m_Name = name;
                m_Value = value;
                m_Attributes = attributes;
            }

            public string GetName() => m_Name.ToString();
            public bool IsReadOnly => true;
            public bool IsContainer => false;
            public IPropertyAttributeCollection Attributes => m_Attributes;
            public bool GetValue(ref SerializedObjectView container) => m_Value.AsBoolean();
            public void SetValue(ref SerializedObjectView container, bool value) => throw new InvalidOperationException("Property is ReadOnly");
        }

        private struct SerializedArrayViewProperty : ICollectionProperty<SerializedObjectView, SerializedArrayView>
        {
            private struct ElementProperty<TElementValue> : ICollectionElementProperty<SerializedObjectView, TElementValue>
            {
                private readonly int m_Index;
                private readonly TElementValue m_Value;
                private readonly IPropertyAttributeCollection m_Attributes;

                public ElementProperty(int index, TElementValue value, bool isContainer, IPropertyAttributeCollection attributes = null)
                {
                    m_Value = value;
                    m_Attributes = attributes;
                    m_Index = index;
                    IsContainer = isContainer;
                }

                public string GetName() => $"[{Index}]";
                public int Index => m_Index;
                public bool IsReadOnly => true;
                public bool IsContainer { get; private set; }
                public IPropertyAttributeCollection Attributes => m_Attributes;
                public TElementValue GetValue(ref SerializedObjectView container) => m_Value;
                public void SetValue(ref SerializedObjectView container, TElementValue value) => throw new InvalidOperationException("Property is ReadOnly");
            }

            private readonly SerializedStringView m_Name;
            private readonly SerializedArrayView m_Value;
            private readonly IPropertyAttributeCollection m_Attributes;

            // @HACK
            private readonly SerializedValueView[] m_Elements;

            public SerializedArrayViewProperty(SerializedStringView name, SerializedArrayView value, IPropertyAttributeCollection attributes = null)
            {
                m_Name = name;
                m_Value = value;
                m_Attributes = attributes;

                // @HACK
                // Since we don't have an efficient indexer. We will pay the allocation cost on creation of the property.
                m_Elements = m_Value.ToArray();
            }

            public string GetName() => m_Name.ToString();
            public bool IsReadOnly => true;
            public bool IsContainer => false;
            public IPropertyAttributeCollection Attributes => m_Attributes;
            public SerializedArrayView GetValue(ref SerializedObjectView container) => m_Value;
            public void SetValue(ref SerializedObjectView container, SerializedArrayView value) => throw new InvalidOperationException("Property is ReadOnly");
            public int GetCount(ref SerializedObjectView container) => m_Value.Count();
            public void SetCount(ref SerializedObjectView container, int count) => throw new InvalidOperationException("Property is ReadOnly");
            public void Clear(ref SerializedObjectView container) => throw new InvalidOperationException("Property is ReadOnly");

            public void GetPropertyAtIndex<TGetter>(ref SerializedObjectView container, int index, ref ChangeTracker changeTracker, TGetter getter)
                where TGetter : ICollectionElementGetter<SerializedObjectView>
            {
                switch (m_Elements[index].Type)
                {
                    case TokenType.Object:
                        getter.VisitProperty<ElementProperty<SerializedObjectView>, SerializedObjectView>(new ElementProperty<SerializedObjectView>(index, m_Elements[index].AsObjectView(), true, m_Attributes), ref container);
                        break;
                    case TokenType.Array:
                        throw new Exception("Arrays of arrays are not supported yet!");
                    case TokenType.String:
                        getter.VisitProperty<ElementProperty<SerializedStringView>, SerializedStringView>(new ElementProperty<SerializedStringView>(index, m_Elements[index].AsStringView(), false, m_Attributes), ref container);
                        break;
                    case TokenType.Primitive:
                    {
                        var p = m_Elements[index].AsPrimitiveView();

                        if (p.IsIntegral())
                        {
                            if (p.IsSigned())
                            {
                                getter.VisitProperty<ElementProperty<long>, long>(new ElementProperty<long>(index, p.AsInt64(), false, m_Attributes), ref container);
                            }
                            else
                            {
                                getter.VisitProperty<ElementProperty<ulong>, ulong>(new ElementProperty<ulong>(index, p.AsUInt64(), false, m_Attributes), ref container);
                            }
                        }
                        else if (p.IsDecimal())
                        {
                            getter.VisitProperty<ElementProperty<float>, float>(new ElementProperty<float>(index, p.AsFloat(), false, m_Attributes), ref container);
                        }
                        else if (p.IsBoolean())
                        {
                            getter.VisitProperty<ElementProperty<bool>, bool>(new ElementProperty<bool>(index, p.AsBoolean(), false, m_Attributes), ref container);
                        }
                    }
                    break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

            }
        }

        static SerializedObjectViewPropertyBag()
        {
            TypeConversion.Register<SerializedValueView, Guid>(v => new Guid(v.AsStringView().ToString()));
            TypeConversion.Register<SerializedStringView, Guid>(v => new Guid(v.ToString()));
            TypeConversion.Register<SerializedStringView, string>(v => v.ToString());
            TypeConversion.Register<SerializedStringView, char>(v => v[0]);
        }

        public override void Accept<TVisitor>(ref SerializedObjectView container, TVisitor visitor, ref ChangeTracker changeTracker)
        {
            foreach (var member in container)
            {
                var nameView = member.Name();
                var valueView = member.Value();

                switch (valueView.Type)
                {
                    case TokenType.Object:
                    {
                        visitor.VisitProperty<SerializedObjectViewProperty, SerializedObjectView, SerializedObjectView>(new SerializedObjectViewProperty(nameView, valueView), ref container, ref changeTracker);
                    }
                    break;

                    case TokenType.Array:
                    {
                        visitor.VisitCollectionProperty<SerializedArrayViewProperty, SerializedObjectView, SerializedArrayView>(new SerializedArrayViewProperty(nameView, valueView.AsArrayView()), ref container, ref changeTracker);
                    }
                    break;

                    case TokenType.String:
                    {
                        visitor.VisitProperty<StringViewProperty, SerializedObjectView, SerializedStringView>(new StringViewProperty(nameView, valueView), ref container, ref changeTracker);
                    }
                    break;

                    case TokenType.Primitive:
                    {
                        var p = valueView.AsPrimitiveView();

                        if (p.IsIntegral())
                        {
                            if (p.IsSigned())
                            {
                                visitor.VisitProperty<Int64Property, SerializedObjectView, long>(new Int64Property(nameView, valueView), ref container, ref changeTracker);
                            }
                            else
                            {
                                visitor.VisitProperty<UInt64Property, SerializedObjectView, ulong>(new UInt64Property(nameView, valueView), ref container, ref changeTracker);
                            }
                        }
                        else if (p.IsDecimal())
                        {
                            visitor.VisitProperty<FloatProperty, SerializedObjectView, float>(new FloatProperty(nameView, valueView), ref container, ref changeTracker);
                        }
                        else if (p.IsBoolean())
                        {
                            visitor.VisitProperty<BoolProperty, SerializedObjectView, bool>(new BoolProperty(nameView, valueView), ref container, ref changeTracker);
                        }
                    }
                    break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public override bool FindProperty<TAction>(string name, ref SerializedObjectView container, ref ChangeTracker changeTracker, ref TAction action)
        {
           if (!container.TryGetMember(name, out var member))
           {
               return false;
           }

           var nameView = member.Name();
           var valueView = member.Value();

           switch (valueView.Type)
           {
               case TokenType.Object:
               {
                   action.VisitProperty<SerializedObjectViewProperty, SerializedObjectView>(new SerializedObjectViewProperty(nameView, valueView), ref container, ref changeTracker);
               }
               break;

               case TokenType.Array:
               {
                   action.VisitCollectionProperty<SerializedArrayViewProperty, SerializedArrayView>(new SerializedArrayViewProperty(nameView, valueView.AsArrayView()), ref container, ref changeTracker);
               }
               break;

               case TokenType.String:
               {
                   action.VisitProperty<StringViewProperty, SerializedStringView>(new StringViewProperty(nameView, valueView), ref container, ref changeTracker);
               }
               break;

               case TokenType.Primitive:
               {
                   var p = valueView.AsPrimitiveView();

                   if (p.IsIntegral())
                   {
                       if (p.IsSigned())
                       {
                           action.VisitProperty<Int64Property, long>(new Int64Property(nameView, valueView), ref container, ref changeTracker);
                       }
                       else
                       {
                           action.VisitProperty<UInt64Property, ulong>(new UInt64Property(nameView, valueView), ref container, ref changeTracker);
                       }
                   }
                   else if (p.IsDecimal())
                   {
                       action.VisitProperty<FloatProperty, float>(new FloatProperty(nameView, valueView), ref container, ref changeTracker);
                   }
                   else if (p.IsBoolean())
                   {
                       action.VisitProperty<BoolProperty, bool>(new BoolProperty(nameView, valueView), ref container, ref changeTracker);
                   }
               }
                   break;

               default:
                   throw new ArgumentOutOfRangeException();
           }

           return true;
        }
    }
}
