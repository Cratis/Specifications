# Specifications

2 passed, 3 failed, 1 skipped in 817 ms

- **Account**
  - **when closing**
    - **and the balance is positive**
      - ❌ The context failed
        - The database was not available
      - ❌ should refund (<1 ms)
      - ❌ should close (<1 ms)
  - **when depositing**
    - ✅ should increase balance (12 ms)
    - ❌ should record deposit (5 ms)
      - Expected: `1`
      - Actual: `2`
    - ⏭️ should notify (skipped: not yet)
- **Customer**
  - **when registering**
    - ✅ should be created (800 ms)
