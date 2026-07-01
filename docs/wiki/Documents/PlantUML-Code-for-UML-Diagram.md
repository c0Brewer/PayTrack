To be adjusted during Development

```uml
@startuml

class User {
  +id: UUID
  --
  name: string
  email: string
  google_id: string
  
  team: Team
  
  role: Role
  is_active: boolean
  
  created_at: datetime
}

enum Role {
  REGULAR_USER
  TEAM_LEAD
  ADMIN
}

class Team {
  +id: UUID
  --
  name: string
  description: string
  display_color: string
  budgets: Budget[]
}

class CostCentre {
  +id: UUID
  --
  name: string
  description: string
  display_color: string
  budgets: Budget[]
}

class Budget {
  +id: UUID
  --
  team_id: UUID
  costcentre_id: UUID
  
  target_amount: number
  period_start: datetime
  period_end: datetime
}

class BankAccount {
  +id: UUID
  --
  user_id: UUID
  iban: string
  bic: string
  account_holder: string
}

abstract class Transaction {
    +id: UUID
    --
    user_id: UUID
    amount: number
    purpose_of_payment: string
    payment_reference: string // set by finance team
    
    status: TransactionStatus
    
    cost_centre_id: UUID
    team_id: UUID
    
    payment_direction: PaymentDirection
    
    created_at: datetime
    paid_at: datetime
}

class PaymentManual {
    payment_source: string
}

class PaymentRequestByUser {
  user_id: UUID
  
  invoice_number: string
  comment: string?
  receipt_url: string (for file storing)
  
  payout_type: PayoutType 
  bank_account_id: UUID (nullable)
}

class TransactionStatusHistory {
  +id: UUID
  --
  transaction_id: UUID
  
  changed_by: UUID
  comment: string
  
  from_status: TransactionStatus
  to_status: TransactionStatus
  changed_at: datetime
}

class PaymentRequestByTeam {
  requested_by: UUID   ' finance/admin user
}

enum TransactionStatus {
  SUBMITTED
  CHANGES_REQUESTED
  APPROVED
  PAID
  DECLINED
}

enum PayoutType {
  USER
  EXTERNAL
}

enum PaymentDirection {
  IN 
  OUT
}

' =======================
' Relationships
' =======================
Transaction <|-- PaymentRequestByTeam
Transaction <|-- PaymentRequestByUser
Transaction <|-- PaymentManual
Transaction "1" -- "0..*" TransactionStatusHistory : has_history
Transaction <.. TransactionStatus
Transaction <.. PaymentDirection
Transaction  "0..*" -- "1" Team: contains
Transaction "0..*" -- "1" CostCentre : categorizes

User "1" -- "0..*" Transaction
User "1" -- "1" Role
User "1" -- "0..*" BankAccount : owns
User "1" -- "0..*" TransactionStatusHistory : changes
User "1" -- "0..*" PaymentRequestByTeam : "requested_by"
User "1" -- "1" Team : "is_part_of"

PaymentRequestByUser --> BankAccount
PaymentRequestByUser <.. PayoutType

Team "1" -- "0..*" Budget
CostCentre "1" -- "0..*" Budget

TransactionStatusHistory <.. TransactionStatus
@enduml
```