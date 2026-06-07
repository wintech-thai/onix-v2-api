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

### Get Payment Transactions
apiUrl = "api/PaymentTransaction/org/#{orgId}/action/GetPaymentTransactions"
param =  {
  FullTextSearch: "",
  Status: "",
  Limit: 10,
  Offset: 0
}

token = File.read(keyFile)

ENV['API_KEY'] = nil
ENV['ACCESS_TOKEN'] = token

puts("===== Get Payment Transactions =====")
result = make_request(:post, apiUrl, param)
puts(result)

### Get Payment Transaction Count
apiUrl = "api/PaymentTransaction/org/#{orgId}/action/GetPaymentTransactionCount"

puts("\n===== Get Payment Transaction Count =====")
result = make_request(:post, apiUrl, param)
puts(result)

### Get Payment Transaction By ID
paymentTransactionId = "YOUR_PAYMENT_TRANSACTION_ID"  # Replace with actual ID
apiUrl = "api/PaymentTransaction/org/#{orgId}/action/GetPaymentTransactionById/#{paymentTransactionId}"

puts("\n===== Get Payment Transaction By ID =====")
result = make_request(:get, apiUrl)
puts(result)
