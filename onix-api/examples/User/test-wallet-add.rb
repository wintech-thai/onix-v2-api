#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']

apiUrl = "api/Point/org/#{orgId}/action/AddWallet"
param =  {
  Name: "WALLET-001",
  Description: "Test wallet manually added",
  Tags: "email=xxxx@gmail.com"
}

result = make_request(:post, apiUrl, param)
puts(result)
