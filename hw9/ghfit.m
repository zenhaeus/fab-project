function [parmhat, se, parmci, output] = ghfit(X, alpha, flag)
%GHFIT Parameter estimates and confidence intervals for generalized
%   hyperbolic data.
%   PARMHAT = GHFIT(X) returns maximum likelihood estimates of
%   the five-parameters generalized hyperbolic (GH) distribution.
%
%   LAMBDA = PARMHAT(1) is a tail index parameter
%   ZETA   = PARMHAT(2) is a shape parameter
%   RHO    = PARMHAT(3) is a skewness parameter
%
%   [PARMHAT,SE] = GHFIT(X) returns approximate standard errors for the
%   parameter estimates.
%
%   [PARMHAT,SE,PARMCI] = GHFIT(X) returns 95% confidence intervals for
%   the parameter estimates.
%
%   [PARMHAT,SE,PARMCI,OUTPUT] = GHFIT(X) further returns a structure
%   OUTPUT containing the fitted skewness and kurtosis, and the maximum
%   log-likelihood.
%
%   [...] = GHFIT(X,ALPHA) returns 100(1-ALPHA) percent
%   confidence intervals for the parameter estimates.
%   Pass in [] for ALPHA to use the default values.
%
%   [...] = GHFIT(X,ALPHA,FLAG), when FLAG = 1 further
%   estimates the mean M, and standard deviation SIGMA given the data in X.
%   {Default FLAG = 0}.
%   PARMHAT(4) is the mean M, and PARMHAT(5) is the standard deviation SIGMA.
%
%   GHFIT needs the Optimization Toolboxes.
%
%   See also SGTFIT, GHPDF, GHCDF.
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%             Copyright (c) 22 November 2014 by Ahmed BenSaïda           %
%                 LaREMFiQ Laboratory, IHEC Sousse - Tunisia             %
%                       Email: ahmedbensaida@yahoo.com                   %
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

if ~isvector(X)
    error(message('Input variable ''X'' must be a vector.'));
end

if nargin < 2 || isempty(alpha)
    alpha = 0.05;
end

if nargin < 3 || isempty(flag) || (flag == 0)
    flag = []; % Ensure flag exists.
else
    % Any number different from zero will give 1.
    flag = logical(flag);
end

% Check if the Optimization toolbox is installed. 1 if installed, 0 if not.
existOptim = license('test','optim_toolbox');

% The default options include turning fmincon's display off.  This
% function gives its own warning/error messages, and the caller can turn
% display on to get the text output from fmincon if desired.

if existOptim
    Options = optimset('fmincon');
else
    Options = optimset('fminsearch');
end

Options = optimset(Options,...
                        'Display'    , 'off',...
                        'MaxFunEvals', 1000 ,...
                        'MaxIter'    , 800  ,...
                        'Diagnostics', 'off',...
                        'Algorithm'  , 'active-set');

% Specify a tolerance level for linear constraints.
global TOLERANCE
TOLERANCE = 2 * optimget(Options,'TolCon',1e-7);

if isa(X,'single')
    X = double(X);
end

% Initial guess of M.
m0     = mean(X);
sigma0 = std(X,0);

%
% Use fsolve to solve skewness and kurtosis for the initial guess of
% LAMBDA, ZETA and RHO. Solve for the stansardized X since it gives more
% consistent results.
%

Lambda0 = 0;
Zeta0   = 8;
Rho0    = 0;

parmhat = [Lambda0, Zeta0, Rho0];

if existOptim
    Opt = optimoptions('fsolve','Display','off','Algorithm',...
    'levenberg-marquardt','ScaleProblem','jacobian');

    parmhat = fsolve(@SkewnessKurtosis, parmhat, Opt, (X - m0)/sigma0);
end

if flag
    % Further estimate the mean and statandard deviantion.
    parmhat = [parmhat, m0, sigma0];
else
    % Standardize the data.
    X = (X - m0) / sigma0;
end

% set lower and upper bounds to avoid overflows in the BESSELK function.
LB = [-Inf, TOLERANCE, -1 + TOLERANCE, -Inf * flag, ...
    TOLERANCE * flag]; % set the lower bounds.

UB = [Inf, 500, 1 - TOLERANCE, Inf * flag, Inf * flag]; % set the upper bounds.

%
% Maximize the log-likelihood with respect to parmhat. Since FMINCON
% minimizes the function, GHTLIKE gives the nagative of the log-likelihood.
%

if existOptim
    [parmhat,logL,err,out] = fmincon(@ghlike, parmhat, [], [], ...
    [], [], LB, UB, [], Options, X);

    % We can also ensure that the estimated parameters satisfy the
    % theoritical skewness and kurtosis of the GH. But this method is not
    % recommended, since it does not give statisfying results.

    % [parmhat,logL,err,out] = fmincon(@ghlike, parmhat, [], [], [], [], LB, ...
    %     UB, @SkewnessKurtosis, Options, X);

else
    % Use a private function FMINSEARCHBND.
    [parmhat,logL,err,out] = fminsearchbnd(@ghlike, parmhat, LB, UB, ...
        Options, X);
end

% Convert minimum negative loglikelihood to maximum likelihood:
logL = -logL;

% Computes the estimated Skewness and Kurtosis.
SkewKurt = SkewnessKurtosis(parmhat, X)' + [skewness(X, 0), kurtosis(X, 0)];

% Construct the OUTPUT structure.
output.LogLikelihood = logL;
output.Skewness      = SkewKurt(1);
output.Kurtosis      = SkewKurt(2);

if (err == 0)
    % fmincon may print its own output text; in any case give something
    % more statistical here, controllable via warning IDs.
    if out.funcCount >= Options.MaxFunEvals
        warning('Number of function evaluation has exceeded the limit.');
    else
        warning('Number of iterations has exceeded the limit.');
    end
elseif (err < 0)
    error('No solution found.');
end

if nargout > 1
    probs = [alpha/2, 1-alpha/2];
    acov  = varcov(parmhat, X);
    se    = sqrt(diag(acov))';

    % Compute the CI for LAMBDA using a normal distribution.
    LambdaCI = norminv(probs, parmhat(1), se(1));
    
    % Because ZETA must be always > 0, compute the CI for ZETA using a
    % normal approximation for log(ZETA), and transform back to the
    % original scale. se(log(ZETA)) is: se(ZETA) / ZETA,
    % (Delta method).
    ZetaCI  = exp(norminv(probs, log(parmhat(2)), se(2)/parmhat(2)));
    
    % Because -1 < RHO < 1, compute the CI for RHO using a
    % normal approximation for atanh(RHO), and transform back to the
    % original scale. se(atanh(RHO)) is: se(RHO) / (1 - RHO^2),
    % (Delta method).
    RhoCI   = tanh(norminv(probs, atanh(parmhat(3)), ...
                se(3)/(1-parmhat(3)^2)));
        
    if flag
        % Compute the CI for M using a normal distribution.
        mCI      = norminv(probs, parmhat(4), se(4));
        
        % Compute the CI for SIGMA using a normal approximation for
        % log(SIGMA), and transform back to the original scale.
        % se(log(SIGMA)) is: se(SIGMA) / SIGMA.
        sigmaCI = exp(norminv(probs, log(parmhat(5)), ...
                    se(5)/parmhat(5)));
    else
        mCI     = [];
        sigmaCI = [];
    end
    
    parmci = [LambdaCI; ZetaCI; RhoCI; mCI; sigmaCI];

end

if isa(X,'single')
    parmhat = single(parmhat);
    if nargout > 1
        parmci = single(parmci);
    end
end

%-------------------------------------------------------------------------%
%                           Helper functions                              %
%-------------------------------------------------------------------------%

function [C, Ceq] = SkewnessKurtosis(parms, x)
% Skewness and Kurtosis of the SGT.

global TOLERANCE

Lambda = parms(1);
Zeta   = parms(2);
Rho    = parms(3);

% Avoid constarints violations.
Zeta(Zeta <= 0) = TOLERANCE;
Rho(Rho <= -1)  = -1 + TOLERANCE;
Rho(Rho >= 1)   =  1 - TOLERANCE;

try
    
    % Skewness and kurtosis of the SGT.
    Delta = 1 / sqrt(besselk(Lambda + 1, Zeta) / (Zeta * ...
          besselk(Lambda, Zeta)) + (Rho^2 / (1 - Rho^2)) * ...
          (besselk(Lambda + 2, Zeta) / besselk(Lambda, Zeta) - ...
          (besselk(Lambda + 1, Zeta) / besselk(Lambda, Zeta))^2));

    Skew = 3 * Delta^3 * Rho / (Zeta * sqrt(1 - Rho^2) * besselk(Lambda, Zeta)) * ...
        (besselk(Lambda+2, Zeta) - besselk(Lambda+1, Zeta)^2 / ...
        besselk(Lambda, Zeta)) + Delta^3 * Rho^3 / (1 - Rho^2)^(3/2) * ...
        (2 * besselk(Lambda+1, Zeta)^3 / besselk(Lambda, Zeta)^3 - 3 * ...
        besselk(Lambda+1, Zeta) * besselk(Lambda+2, Zeta) / besselk(Lambda, Zeta)^2 + ...
        besselk(Lambda+3, Zeta) / besselk(Lambda, Zeta));
        
    Kurt = 6 * Delta^4 * Rho^2 * besselk(Lambda+1, Zeta)^2 / ((1 - Rho^2) * ...
        besselk(Lambda, Zeta)^3) * (besselk(Lambda+1, Zeta) / Zeta + ...
        Rho^2 * besselk(Lambda+2, Zeta) / (1 - Rho^2)) - 3 * Delta^4 * ...
        Rho^4 * besselk(Lambda+1, Zeta)^4 / ((1 - Rho^2)^2 * besselk(Lambda, Zeta)^4) + ...
        Delta^4 * (3 - 6 * Rho^2 + (3 + Zeta^2) * Rho^4) * besselk(Lambda+2, Zeta) / ...
        (Zeta^2 * (1 - Rho^2)^2 * besselk(Lambda, Zeta)) + 2 * Delta^4 * Rho^4 * ...
        (3 + Lambda * Rho^2) * besselk(Lambda+3, Zeta) / (Zeta * (1 - Rho^2)^2 * ...
        besselk(Lambda, Zeta)) - 4 * Delta^4 * Rho^2 * besselk(Lambda+1, Zeta) / ...
        ((1 - Rho^2) * besselk(Lambda, Zeta)^2) * (3 * besselk(Lambda+2, Zeta) / ...
        Zeta + Rho^2 * besselk(Lambda+3, Zeta) / (1 - Rho^2));
   
catch
    % For some parameters, the theoritical skewness and kurtosis cannot be
    % computed, so give a reasonable answer.
    
    Skew = skewness(x, 0);
    Kurt = kurtosis(x, 0);
    
end

% Nonlinear equality constraint.
Ceq = [Skew - skewness(x, 0); Kurt - kurtosis(x, 0)];

if nargout >= 2
    % Use with FMINCON.
    C = [0; 0]; % Nonlinear inequality constraint.
else
    % Use with FSOLVE for initial parameters guess.
    C   = Ceq;
    Ceq = [];
end

%-------------------------------------------------------------------------%

function LogL = ghlike(parms, x)
% GH negative log-likelihood.

global TOLERANCE

Lambda = parms(1);
Zeta   = parms(2);
Rho    = parms(3);

% Avoid constarints violations.
Zeta(Zeta <= 0) = TOLERANCE;
Rho(Rho <= -1)  = -1 + TOLERANCE;
Rho(Rho >= 1)   =  1 - TOLERANCE;

if length(parms) > 3
    m      = parms(4);
    sigma  = parms(5);
else
    m     = 0;
    sigma = 1;
end

LogL = zeros(size(x));

Delta = 1 / sqrt(besselk(Lambda + 1, Zeta) / (Zeta * ...
      besselk(Lambda, Zeta)) + (Rho^2 / (1 - Rho^2)) * ...
      (besselk(Lambda + 2, Zeta) / besselk(Lambda, Zeta) - ...
      (besselk(Lambda + 1, Zeta) / besselk(Lambda, Zeta))^2));
    
mu   = m -  sigma * Delta * Rho / sqrt(1 - Rho^2) * ...
     besselk(Lambda + 1, Zeta) / besselk(Lambda, Zeta);

xx   = ((x - mu) / (sigma * Delta));

LogL(:) = log(2*pi)/2 + log(sigma) + log(Delta) + log(besselk(Lambda, Zeta)) - ... 
    (Lambda/2 - 0.25) * log(1 - Rho^2) - log(Zeta)/2 - (Lambda/2 - 0.25) * ...
     log(1 + xx.^2) - log(besselk(Lambda - 1/2, Zeta / sqrt(1 - Rho^2) * ...
     sqrt(1 + xx.^2))) - (Zeta * Rho / sqrt(1 - Rho^2) * xx);
 
% The log-likelihood obtained is the negative of the real LogL.
LogL = sum(LogL);

%
% Catch conditions that produce anomalous log-likelihood function values.
% Typically, what happens is that input parameter values will result in
% an unstable inverse filter, which produces LogL = inf. This, in turn, will
% not allow FMINCON to update the iteration. By setting the LogL to a large, 
% but finite, value, we can safeguard against anomalies and allows the 
% iteration to continue.
%

LogL(~isfinite(LogL))  =  1.0e+20;
LogL(~(~imag(LogL)))   =  1.0e+20;

%-------------------------------------------------------------------------
function covarianceMatrix = varcov(parmhat, y)                               
%VARCOV Error covariance matrix of maximum likelihood parameter estimates

delta = 1e-10;  % Offset for numerical differentiation

% Evaluate the loglikelihood objective function at the MLE parameter
% estimates. In contrast to the optimization, which is interested in the
% single scalar objective function value logL, here we are interested in
% the loglikelihoods for each observation of y(t), which sum to -logL.

LogLikelihoods = ghlike(parmhat, y);

g0 = -LogLikelihoods;

% Initialize the perturbed parameter vector and the scores matrix. For T
% observations in y(t) and K parameters estimated via maximum likelihood,
% the scores array is a T-by-K matrix.

pDelta = parmhat;
scores = zeros(length(y),numel(parmhat));

for j = 1:numel(parmhat)

    pDelta(j) = parmhat(j)*(1+delta);
    dp = delta*parmhat(j);

    % Trap the case of a zero parameter value, p0(j) = 0:

    if dp == 0
        dp = delta;
        pDelta(j) = dp;
    end

    LogLikelihoods = ghlike(pDelta, y);

    gDelta = -LogLikelihoods;

    scores(:,j) = (g0-gDelta)/dp;
    pDelta(j) = parmhat(j);

end

% Invert the outer product of the scores matrix to get the approximate
% covariance matrix of the MLE parameters.

try

  % Pre-allocate the output covariance matrix to the full size.
   covarianceMatrix = zeros(numel(parmhat));
   
   j = 1:numel(parmhat);
   covarianceMatrix(j,j) = pinv(scores(:,j)'*scores(:,j));
   
catch

  % If an error occurs in the calculation of the covariance matrix, then
  % assign a matrix of all NaNs to indicate the error condition:

   covarianceMatrix = NaN(numel(parmhat),numel(parmhat));

end

covarianceMatrix = (covarianceMatrix + covarianceMatrix')/2;
