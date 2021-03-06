﻿#pragma warning disable 0649
//------------------------------------------------------------------------------
// <auto-generated>This code was generated by LLBLGen Pro v4.2.</auto-generated>
//------------------------------------------------------------------------------
using System;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.ComponentModel;

namespace WriteTests.EntityClasses
{
	/// <summary>Class which represents the entity 'Customer', mapped on table 'LLBLGenProUnitTest.dbo.Customer'.</summary>
	public partial class Customer : INotifyPropertyChanging, INotifyPropertyChanged
	{
		#region Events
		/// <summary>Event which is raised when a property value is changing.</summary>
		public event PropertyChangingEventHandler PropertyChanging;
		/// <summary>Event which is raised when a property value changes.</summary>
		public event PropertyChangedEventHandler PropertyChanged;
		#endregion
		
		#region Class Member Declarations
		private System.Int32	_billingAddressId;
		private System.String	_companyEmailAddress;
		private System.String	_companyName;
		private System.String	_contactPerson;
		private System.Int32	_customerId;
		private System.DateTime	_customerSince;
		private System.Guid	_testRunId;
		private System.Int32	_visitingAddressId;
		private EntityRef <Address> _billingAddress;
		private EntityRef <Address> _visitingAddress;
		private EntitySet <Order> _orders;
		private EntityRef <SpecialCustomer> _specialCustomer;
		#endregion
		
		#region Extensibility Method Definitions
		partial void OnLoaded();
		partial void OnValidate(System.Data.Linq.ChangeAction action);
		partial void OnCreated();
		partial void OnBillingAddressIdChanging(System.Int32 value);
		partial void OnBillingAddressIdChanged();
		partial void OnCompanyEmailAddressChanging(System.String value);
		partial void OnCompanyEmailAddressChanged();
		partial void OnCompanyNameChanging(System.String value);
		partial void OnCompanyNameChanged();
		partial void OnContactPersonChanging(System.String value);
		partial void OnContactPersonChanged();
		partial void OnCustomerIdChanging(System.Int32 value);
		partial void OnCustomerIdChanged();
		partial void OnCustomerSinceChanging(System.DateTime value);
		partial void OnCustomerSinceChanged();
		partial void OnTestRunIdChanging(System.Guid value);
		partial void OnTestRunIdChanged();
		partial void OnVisitingAddressIdChanging(System.Int32 value);
		partial void OnVisitingAddressIdChanged();
		#endregion
		
		/// <summary>Initializes a new instance of the <see cref="Customer"/> class.</summary>
		public Customer()
		{
			_billingAddress = default(EntityRef<Address>);
			_visitingAddress = default(EntityRef<Address>);
			_orders = new EntitySet<Order>(new Action<Order>(this.Attach_Orders), new Action<Order>(this.Detach_Orders) );
			_specialCustomer = default(EntityRef<SpecialCustomer>);
			OnCreated();
		}

		/// <summary>Raises the PropertyChanging event</summary>
		/// <param name="propertyName">name of the property which is changing</param>
		protected virtual void SendPropertyChanging(string propertyName)
		{
			if((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
			}
		}
		
		/// <summary>Raises the PropertyChanged event for the property specified</summary>
		/// <param name="propertyName">name of the property which was changed</param>
		protected virtual void SendPropertyChanged(string propertyName)
		{
			if((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
		
		/// <summary>Attaches this instance to the entity specified as an associated entity</summary>
		/// <param name="entity">The related entity to attach to</param>
		private void Attach_Orders(Order entity)
		{
			this.SendPropertyChanging("Orders");
			entity.Customer = this;
		}
		
		/// <summary>Detaches this instance from the entity specified so it's no longer an associated entity</summary>
		/// <param name="entity">The related entity to detach from</param>
		private void Detach_Orders(Order entity)
		{
			this.SendPropertyChanging("Orders");
			entity.Customer = null;
		}


		#region Class Property Declarations
		/// <summary>Gets or sets the BillingAddressId field. Mapped on target field 'BillingAddressID'. </summary>
		public System.Int32 BillingAddressId
		{
			get	{ return _billingAddressId; }
			set
			{
				if((_billingAddressId != value))
				{
					if(_billingAddress.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					OnBillingAddressIdChanging(value);
					SendPropertyChanging("BillingAddressId");
					_billingAddressId = value;
					SendPropertyChanged("BillingAddressId");
					OnBillingAddressIdChanged();
				}
			}
		}

		/// <summary>Gets or sets the CompanyEmailAddress field. Mapped on target field 'CompanyEmailAddress'. </summary>
		public System.String CompanyEmailAddress
		{
			get	{ return _companyEmailAddress; }
			set
			{
				if((_companyEmailAddress != value))
				{
					OnCompanyEmailAddressChanging(value);
					SendPropertyChanging("CompanyEmailAddress");
					_companyEmailAddress = value;
					SendPropertyChanged("CompanyEmailAddress");
					OnCompanyEmailAddressChanged();
				}
			}
		}

		/// <summary>Gets or sets the CompanyName field. Mapped on target field 'CompanyName'. </summary>
		public System.String CompanyName
		{
			get	{ return _companyName; }
			set
			{
				if((_companyName != value))
				{
					OnCompanyNameChanging(value);
					SendPropertyChanging("CompanyName");
					_companyName = value;
					SendPropertyChanged("CompanyName");
					OnCompanyNameChanged();
				}
			}
		}

		/// <summary>Gets or sets the ContactPerson field. Mapped on target field 'ContactPerson'. </summary>
		public System.String ContactPerson
		{
			get	{ return _contactPerson; }
			set
			{
				if((_contactPerson != value))
				{
					OnContactPersonChanging(value);
					SendPropertyChanging("ContactPerson");
					_contactPerson = value;
					SendPropertyChanged("ContactPerson");
					OnContactPersonChanged();
				}
			}
		}

		/// <summary>Gets or sets the CustomerId field. Mapped on target field 'CustomerID'. </summary>
		public System.Int32 CustomerId
		{
			get	{ return _customerId; }
			set
			{
				if((_customerId != value))
				{
					OnCustomerIdChanging(value);
					SendPropertyChanging("CustomerId");
					_customerId = value;
					SendPropertyChanged("CustomerId");
					OnCustomerIdChanged();
				}
			}
		}

		/// <summary>Gets or sets the CustomerSince field. Mapped on target field 'CustomerSince'. </summary>
		public System.DateTime CustomerSince
		{
			get	{ return _customerSince; }
			set
			{
				if((_customerSince != value))
				{
					OnCustomerSinceChanging(value);
					SendPropertyChanging("CustomerSince");
					_customerSince = value;
					SendPropertyChanged("CustomerSince");
					OnCustomerSinceChanged();
				}
			}
		}

		/// <summary>Gets or sets the TestRunId field. Mapped on target field 'TestRunID'. </summary>
		public System.Guid TestRunId
		{
			get	{ return _testRunId; }
			set
			{
				if((_testRunId != value))
				{
					OnTestRunIdChanging(value);
					SendPropertyChanging("TestRunId");
					_testRunId = value;
					SendPropertyChanged("TestRunId");
					OnTestRunIdChanged();
				}
			}
		}

		/// <summary>Gets or sets the VisitingAddressId field. Mapped on target field 'VisitingAddressID'. </summary>
		public System.Int32 VisitingAddressId
		{
			get	{ return _visitingAddressId; }
			set
			{
				if((_visitingAddressId != value))
				{
					if(_visitingAddress.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					OnVisitingAddressIdChanging(value);
					SendPropertyChanging("VisitingAddressId");
					_visitingAddressId = value;
					SendPropertyChanged("VisitingAddressId");
					OnVisitingAddressIdChanged();
				}
			}
		}

		/// <summary>Represents the navigator which is mapped onto the association 'Customer.BillingAddress - Address.CustomerBillingAddress (1:1)'</summary>
		public Address BillingAddress
		{
			get { return _billingAddress.Entity; }
			set
			{
				Address previousValue = _billingAddress.Entity;
				if((previousValue != value) || (_billingAddress.HasLoadedOrAssignedValue == false))
				{
					this.SendPropertyChanging("BillingAddress");
					if(previousValue != null)
					{
						_billingAddress.Entity = null;
						previousValue.CustomerBillingAddress=null;
					}
					_billingAddress.Entity = value;
					if(value==null)
					{
						_billingAddressId = default(System.Int32);
					}
					else
					{
						value.CustomerBillingAddress = this;
						_billingAddressId = value.AddressId;
					}
					this.SendPropertyChanged("BillingAddress");
				}
			}
		}
		
		/// <summary>Represents the navigator which is mapped onto the association 'Customer.VisitingAddress - Address.CustomerVisitingAddress (1:1)'</summary>
		public Address VisitingAddress
		{
			get { return _visitingAddress.Entity; }
			set
			{
				Address previousValue = _visitingAddress.Entity;
				if((previousValue != value) || (_visitingAddress.HasLoadedOrAssignedValue == false))
				{
					this.SendPropertyChanging("VisitingAddress");
					if(previousValue != null)
					{
						_visitingAddress.Entity = null;
						previousValue.CustomerVisitingAddress=null;
					}
					_visitingAddress.Entity = value;
					if(value==null)
					{
						_visitingAddressId = default(System.Int32);
					}
					else
					{
						value.CustomerVisitingAddress = this;
						_visitingAddressId = value.AddressId;
					}
					this.SendPropertyChanged("VisitingAddress");
				}
			}
		}
		
		/// <summary>Represents the navigator which is mapped onto the association 'Order.Customer - Customer.Orders (m:1)'</summary>
		public EntitySet<Order> Orders
		{
			get { return this._orders; }
			set { this._orders.Assign(value); }
		}
		
		/// <summary>Represents the navigator which is mapped onto the association 'SpecialCustomer.Customer - Customer.SpecialCustomer (1:1)'</summary>
		public SpecialCustomer SpecialCustomer
		{
			get { return _specialCustomer.Entity; }
			set
			{
				SpecialCustomer previousValue = _specialCustomer.Entity;
				if((previousValue != value) || (_specialCustomer.HasLoadedOrAssignedValue == false))
				{
					this.SendPropertyChanging("SpecialCustomer");
					if(previousValue != null)
					{
						_specialCustomer.Entity = null;
						previousValue.Customer=null;
					}
					_specialCustomer.Entity = value;
					if(value != null)
					{
						value.Customer = this;
					}
					this.SendPropertyChanged("SpecialCustomer");
				}
			}
		}
		
		/// <summary>Represents the related field 'this.BillingAddress.Country'</summary>
		public System.String BillingAddressCountry
		{
			get { return this.BillingAddress==null ? default(System.String) : this.BillingAddress.Country; }
			set
			{
				if(this.BillingAddress!=null)
				{
					this.BillingAddress.Country = value;
				}
			}
		}

		#endregion
	}
}
#pragma warning restore 0649