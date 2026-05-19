#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']
keyFile = ".token"
bankAccountId = 'a0ebc677-6aa3-4454-9e7e-ee1aa6361e4b'

hhmmss = Time.now.strftime("%H%M%S")

### 
apiUrl = "admin-api/AdminPaymentTx/org/global/action/SubmitLinePaymentTxNotification/#{bankAccountId}"
param =  {
  PaymentAmount: 234.65, #ต้อง match กับ payment request ก่อนหน้าด้วย
  RemainAmount: 0.00,
  TxType: "PayIn",
  SourceBankCode: "KTB",
  SourceBankAccountNo: "XX-0987",
  DestinationBankCode: "TMB",
  DestinationAccountNo: "XX-9032",
}

token = File.read(keyFile)

ENV['API_KEY'] = ENV['PAYMENT_LINE_NOTI_KEY'] # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil
#ENV['ACCESS_TOKEN'] = token

#puts("===[#{token}]")

result = make_request(:post, apiUrl, param)
puts(result)
