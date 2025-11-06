#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env(".env")

orgId = ENV['API_ORG']
itemId = "589df813-2b4e-46c5-b8a5-ad4259330a76"

apiUrl = "api/Privilege/org/#{orgId}/action/GetPrivilegeBalanceById/#{itemId}"
param =  {
  BalanceType: "ItemBalanceCurrent"
}

result = make_request(:post, apiUrl, param)
puts(result)
