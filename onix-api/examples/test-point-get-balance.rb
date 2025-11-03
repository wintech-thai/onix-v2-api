#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env(".env")

orgId = ENV['API_ORG']
walletId = "WALLET-000001"

apiUrl = "api/Point/org/#{orgId}/action/GetPointBalanceByWalletId/#{walletId}"
param =  {
  BalanceType: "PointBalanceCurrent"
}

result = make_request(:post, apiUrl, param)
puts(result)
