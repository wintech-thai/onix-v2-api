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

### Get PayIn Bank Accounts For Merchant
apiUrl = "api/BankAccount/org/#{orgId}/action/GetPayInBankAccountsForMerchant"

token = File.read(keyFile)

ENV['API_KEY'] = nil
ENV['ACCESS_TOKEN'] = token

puts("===== Get PayIn Bank Accounts For Merchant =====")
result = make_request(:get, apiUrl)
puts(result)
