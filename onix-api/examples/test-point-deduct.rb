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

apiUrl = "api/Point/org/#{orgId}/action/DeductPoint/#{walletId}"
param =  {
  TxAmount: 7,
  Description: "This is product deduction # 3",
  Tags: "source=coupon-purchase"
}

result = make_request(:post, apiUrl, param)
puts(result)
