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

token = File.read(keyFile)

ENV['API_KEY'] = nil
ENV['ACCESS_TOKEN'] = token

# ─── GetPayInBankAccountsForMerchant ────────────────────────────────────────
apiUrl = "api/BankAccount/org/#{orgId}/action/GetPayInBankAccountsForMerchant"
result = make_request(:get, apiUrl, nil)
puts(result)

# ─── GetPayOutBankAccountsForMerchant ───────────────────────────────────────
apiUrl = "api/BankAccount/org/#{orgId}/action/GetPayOutBankAccountsForMerchant"
result = make_request(:get, apiUrl, nil)
puts(result)
