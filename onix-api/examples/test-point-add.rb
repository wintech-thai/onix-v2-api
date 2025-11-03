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

apiUrl = "api/Point/org/#{orgId}/action/AddPoint/#{walletId}"
param =  {
  TxAmount: 150,
  Description: "This is product description # 2",
  Tags: "source=cutomer-register"
}

result = make_request(:post, apiUrl, param)
puts(result)
