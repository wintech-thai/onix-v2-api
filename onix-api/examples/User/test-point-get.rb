#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']
walletId = "WALLET-000001"

apiUrl = "api/Point/org/#{orgId}/action/GetPointTxsByWalletId/#{walletId}"
param =  {}

result = make_request(:post, apiUrl, param)
puts(result)

apiUrl = "api/Point/org/#{orgId}/action/GetPointTxsCountByWalletId/#{walletId}"
result = make_request(:post, apiUrl, param)
puts(result)
