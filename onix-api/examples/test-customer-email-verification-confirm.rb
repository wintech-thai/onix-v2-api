#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require 'cgi'
require 'base64'
require './utils'

$stdout.sync = true

################### Main #######################
load_env(".env")

url = "https://register-dev.please-scan.com/napbiotec/customer-email-virification/3ef72ddb-39f8-49bb-a512-261e2d14bd3a?data=eyJFbWFpbCI6ImFiY2RlMkB4eXouY29tIiwiTmFtZSI6Ilx1MEUxRVx1MEUzNVx1MEU0OFx1MEU0MFx1MEUwOFx1MEUyMVx1MEUyQVx1MEU0QyBcdTBFMjNcdTBFMzFcdTBFMDFcdTBFMjlcdTBFNENcdTBFMThcdTBFMjNcdTBFMjNcdTBFMjFcdTBFMEFcdTBFMzJcdTBFMTVcdTBFMzQiLCJDb2RlIjoiQ1VTVC0wMDMiLCJJZCI6IjM3NWVkNDY4LTBlYjEtNGJiMS04MWFlLTRiMzMyNzZhMzg4OSJ9"
uri = URI.parse(url)

# แปลง query string เป็น hash
params = CGI.parse(uri.query)
data = params['data'].first

path = uri.path
parts = path.split('/').reject(&:empty?)

decoded = Base64.decode64(data)
dataObj = JSON.parse(decoded)

orgId = parts[0]
regType = parts[1]
token = parts[2]

api = "ConfirmCustomerEmailVerification"

#puts(dataObj)
custId = dataObj['Id']

param = nil
#puts(param)
#puts(parts)

ENV['API_KEY'] = nil # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil

apiUrl = "api/Registration/org/#{orgId}/action/#{api}/#{token}/#{custId}"
#puts(apiUrl)

result = make_request(:post, apiUrl, param)
puts(result)
