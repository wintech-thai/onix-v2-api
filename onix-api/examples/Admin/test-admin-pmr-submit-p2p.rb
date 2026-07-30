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
merchantId = 'eab2eae2-ab83-4d49-bff6-a30226663d09'

hhmmss = Time.now.strftime("%H%M%S")

### 
apiUrl = "admin-api/AdminPaymentRequest/org/global/action/SubmitPaymentRequestByMerchantIdP2P/#{merchantId}"
param =  {
  RefId1: "XA-MSB-0001922-#{hhmmss}",
  RefId2: "",
  RefId3: "",
  Description: "ทดสอบยิง payment request เข้ามาเฉย ๆ",
  CustomerEmail: "",
  CustomerPhone: "",
  Currency: "THB",
  BankAccountNo: "",
  BankAccountName: "",
  RequestedAmount: 20,
  QrProvider: "PP",
  Tags: "testing",
  #SelectedPayInBankAccountId:
}

token = File.read(keyFile)

ENV['API_KEY'] = nil # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil
ENV['ACCESS_TOKEN'] = token

#puts("===[#{token}]")

result = make_request(:post, apiUrl, param)
puts(result.to_json)
