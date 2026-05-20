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
webhookId = '20bcc00b-a4a5-4206-8dbf-eee3df98b518'
orgId = 'ppm-alfa999'

### 
#apiUrl = "admin-api/AdminWebhookConfig/org/global/action/EnableWebhookConfigById/#{orgId}/#{webhookId}"
apiUrl = "admin-api/AdminWebhookConfig/org/global/action/DisableWebhookConfigById/#{orgId}/#{webhookId}"
param = {}

token = File.read(keyFile)

ENV['API_KEY'] = nil # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil
ENV['ACCESS_TOKEN'] = token

#puts("===[#{token}]")

result = make_request(:post, apiUrl, param)
puts(result)
