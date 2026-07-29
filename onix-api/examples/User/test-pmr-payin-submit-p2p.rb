#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'
require 'faye/websocket'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = 'gabx01'
keyFile = ".token"
merchantId = "eab2eae2-ab83-4d49-bff6-a30226663d09"

hhmmss = Time.now.strftime("%H%M%S")

### Get Payment Requests
apiUrl = "api/PaymentRequest/org/#{orgId}/action/SubmitPayInRequestP2P/#{merchantId}"
param =  {
  RefId: "XA-MSB-0001922-#{hhmmss}",
  RefId1: "INVOICE-0011223-#{hhmmss}",
  RefId2: "ORDER-XSWKEKEI-#{hhmmss}",
  Description: "ทดสอบยิง payment request เข้ามาเฉย ๆ",
  CustomerEmail: "",
  CustomerPhone: "",
  Currency: "THB",
  BankAccountNo: "",
  BankAccountName: "",
  RequestedAmount: 325,
  QrProvider: "PP",
  Tags: "testing",
  #SelectedPayInBankAccountId:
}


token = File.read(keyFile)

ENV['API_KEY'] = ENV['PAYIN_REQUEST_API_KEY'] # ถ้าใช้ API KEY ก็เซ็ตเป็นค่าเดิมที่อ่านมาจากไฟล์
#ENV['ACCESS_TOKEN'] = nil

puts("===== Submit Payment Request =====")
result = make_request(:post, apiUrl, param)
puts(result.to_json)

sessionId = result["sessionId"]
wsPath    = result["websocketPath"]